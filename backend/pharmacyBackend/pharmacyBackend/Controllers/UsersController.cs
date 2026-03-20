using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;
using pharmacyBackend.Models;
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
        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            var defaultPassword = _configuration["DefaultPassword:Password"];
            user.Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Пароль сброшен на: {defaultPassword}" });
        }
    }
}
