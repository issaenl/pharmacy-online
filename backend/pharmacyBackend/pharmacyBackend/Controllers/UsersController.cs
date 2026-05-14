using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;
using pharmacyBackend.Models;
using pharmacyBackend.Services;
using PhoneNumbers;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public UsersController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpGet("customers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomers()
        {
            var customers = await _context.Users
                .Where(u => u.Role == UserRole.User)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.IsBanned,
                    OrdersCount = _context.Orders.Count(o => o.UserId == u.Id)
                })
                .ToListAsync();

            return Ok(customers);
        }

        [HttpPut("ban/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleBanStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (user.Role != UserRole.User)
            {
                return BadRequest("Банить можно только обычных покупателей");
            }

            user.IsBanned = !user.IsBanned;
            await _context.SaveChangesAsync();

            var statusStr = user.IsBanned ? "заблокирован" : "разблокирован";
            return Ok(new { message = $"Пользователь успешно {statusStr}" });
        }

        [HttpDelete("delete-customer/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (user.Role != UserRole.User)
            {
                return BadRequest("Этот метод предназначен только для удаления обычных покупателей");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Аккаунт покупателя успешно удален" });
        }


        [HttpPut("reset-customer-password/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetCustomerPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (user.Role != UserRole.User)
            {
                return BadRequest(new { message = "Этот метод предназначен только для покупателей" });
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { message = "У этого покупателя не указан email. Восстановление пароля невозможно." });
            }

            var newPassword = GenerateRandomPassword(10);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            var emailBody = $@"
                <h3>Сброс пароля</h3>
                <p>Здравствуйте, {user.FirstName}!</p>
                <p>Ваш пароль был успешно сброшен администратором. Ваш новый пароль для входа:</p>
                <h2>{newPassword}</h2>
                <p>Рекомендуем изменить этот пароль в личном кабинете после входа и удалить это письмо.</p>";

            await _emailService.SendEmailAsync(user.Email, "Сброс пароля", emailBody);

            return Ok(new { message = "Новый пароль успешно сгенерирован и отправлен покупателю на email" });
        }


        [HttpGet("admins")]
        public async Task<ActionResult<IEnumerable<AdminDTO>>> GetEmployees()
        {
            var employees = await _context.Users
                .Include(u => u.Pharmacy)
                .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.PharmacyAdmin)
                .Select(u => new AdminDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    PharmacyId = u.PharmacyId,
                    PharmacyName = u.Pharmacy != null ? u.Pharmacy.Name : null
                })
                .ToListAsync();

            return Ok(employees);
        }


        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateEmployee(CreateAdminDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Этот Email уже используется");
            }

            if (dto.Role == UserRole.PharmacyAdmin && dto.PharmacyId == null)
            {
                return BadRequest("Для администратора аптеки необходимо указать ID аптеки");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                PharmacyId = dto.Role == UserRole.Admin ? null : dto.PharmacyId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPut("edit-admin/{id}")]
        public async Task<ActionResult> EditEmployee(int id, UpdateAdminDTO dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (user.Email == "admin@unimed.pharmacy")
            {
                if (dto.Email != user.Email || dto.Role != UserRole.Admin)
                {
                    return BadRequest("Нельзя изменить Email или роль главного администратора");
                }
            }

            if (user.Email != dto.Email && await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Этот Email уже используется другим сотрудником");
            }

            if (dto.Role == UserRole.PharmacyAdmin && dto.PharmacyId == null)
            {
                return BadRequest("Для администратора аптеки необходимо указать ID аптеки");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            user.PharmacyId = dto.Role == UserRole.Admin ? null : dto.PharmacyId;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("delete-admin/{id}")]
        public async Task<ActionResult> DeleteEmployee(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (user.Email == "admin@unimed.pharmacy")
            {
                return BadRequest("Удаление главного системного администратора запрещено");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/reset-password")]
        public async Task<ActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (user.Email == "admin@unimed.pharmacy")
            {
                return BadRequest(new { message = "Сброс пароля главного системного администратора запрещен" });
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { message = "У этого администратора не указан email. Восстановление пароля невозможно." });
            }

            var newPassword = GenerateRandomPassword(12);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            var emailBody = $@"
                <h3>Данные для входа обновлены</h3>
                <p>Здравствуйте, {user.FirstName}!</p>
                <p>Ваш пароль был успешно сброшен администратором. Ваш новый пароль для входа:</p>
                <h2>{newPassword}</h2>
                <p>Рекомендуем изменить этот пароль в личном кабинете после входа и удалить это письмо..</p>";

            await _emailService.SendEmailAsync(user.Email, "Новый пароль администратора", emailBody);

            return Ok(new { message = "Новый пароль сгенерирован и отправлен сотруднику на email" });
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
