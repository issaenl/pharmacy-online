using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;
using pharmacyBackend.Models;
using pharmacyBackend.Services;
using PhoneNumbers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDTO newUser)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            try
            {
                var phoneNumber = phoneNumberUtil.Parse(newUser.Phone, "BY");

                if(!phoneNumberUtil.IsValidNumberForRegion(phoneNumber, "BY"))
                {
                    return BadRequest("Регистрация возможно только через белорусский номер телефона");
                }

                newUser.Phone = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException)
            {
                return BadRequest("Некорректный формат номера телефона");
            }


            if (await _context.Users.AnyAsync(u => u.Phone == newUser.Phone))
            {
                return BadRequest("Такой номер телефона уже используется");
            }

            var user = new User
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Phone = newUser.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                Role = UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDTO loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == loginUser.Login || u.Email == loginUser.Login);

            if(user == null || !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
            {
                return Unauthorized("Неверный пароль или номер телефона");
            }

            var token = CreateToken(user);
            var userData = _mapper.Map<UserDataDTO>(user);

            return Ok(new { Token = token, User = userData });
        }

        private string CreateToken(User user)
        {
            var claimsList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if(user.PharmacyId.HasValue)
            {
                claimsList.Add(new Claim("PharmacyId", user.PharmacyId.Value.ToString()));
            }

            var tokenKey = _configuration.GetValue<string>("AppSettings:Token");
            if (string.IsNullOrEmpty(tokenKey))
            {
                throw new Exception("JWT отсутствует в конфигурации");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                claims: claimsList,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<ActionResult<UserDataDTO>> UpdateProfile (UserUpdateDTO newData)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("Пользователь не найден");

            if (user.Phone != newData.Phone && await _context.Users.AnyAsync(u => u.Phone == newData.Phone))
            {
                return BadRequest("Этот номер телефона уже занят");
            }

            user.FirstName = newData.FirstName;
            user.LastName = newData.LastName;
            user.Phone = newData.Phone;
            user.Email = newData.Email;
            await _context.SaveChangesAsync();  
            return Ok(_mapper.Map<UserDataDTO>(user));
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDTO newData)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("Пользователь не найден");

            if (!BCrypt.Net.BCrypt.Verify(newData.OldPassword, user.Password))
            {
                return BadRequest("Старый пароль неверен");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newData.NewPassword);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteAccount()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("Пользователь не найден");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new {Message = "Аккаунт удален"});
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDataDTO>> GetMe()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("Пользователь не найден");

            return Ok(_mapper.Map<UserDataDTO>(user));
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Ok(new { Message = "Если email существует в системе, на него отправлена ссылка для восстановления." });
            }

            user.ResetPasswordToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendUrl = _configuration["AllowedOrigins"]?.Split(',')[0] ?? "http://localhost:5173";
            var resetLink = $"{frontendUrl}/reset-password?token={user.ResetPasswordToken}&email={user.Email}";

            var emailBody = $@"
                <h3>Восстановление пароля</h3>
                <p>Вы запросили сброс пароля. Пожалуйста, перейдите по ссылке ниже, чтобы установить новый пароль:</p>
                <a href='{resetLink}'>Сбросить пароль</a>
                <p>Ссылка действительна в течение 1 часа.</p>";

            await _emailService.SendEmailAsync(user.Email, "Восстановление пароля", emailBody);

            return Ok(new { Message = "Если email существует в системе, на него отправлена ссылка для восстановления." });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.ResetPasswordToken == request.Token);

            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Неверная или устаревшая ссылка для сброса пароля.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Пароль успешно изменен. Теперь вы можете войти." });
        }

    }
}
