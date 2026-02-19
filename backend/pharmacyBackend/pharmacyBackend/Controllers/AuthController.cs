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
using PhoneNumbers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == loginUser.Phone);

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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Не удалось получить ID пользователя из токена. Пожалуйста, перезайдите в аккаунт.");
            }

            var userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if(user.Phone != newData.Phone && await _context.Users.AnyAsync(u => u.Phone == newData.Phone))
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Не удалось получить ID пользователя из токена. Пожалуйста, перезайдите в аккаунт.");
            }

            var userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if(!BCrypt.Net.BCrypt.Verify(newData.OldPassword, user.Password))
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Не удалось получить ID пользователя из токена. Пожалуйста, перезайдите в аккаунт.");
            }

            var userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new {Message = "Аккаунт удален"});
        }
    }
}
