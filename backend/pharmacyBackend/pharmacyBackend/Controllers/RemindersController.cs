using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Models;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RemindersController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RemindersController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetMyReminders()
        {
            var userId = GetUserId();
            var reminders = await _context.MedicationReminders
                .Include(r => r.Logs)
                .Where(r => r.UserId == userId)
                .ProjectTo<ReminderDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(reminders);
        }

        [HttpPost]
        public async Task<ActionResult> CreateReminder([FromBody] ReminderCreateDTO dto)
        {
            var reminder = new MedicationReminder
            {
                UserId = GetUserId(),
                ProductId = dto.ProductId,
                MedicationName = dto.MedicationName,
                Dosage = dto.Dosage,
                StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc),
                TimesOfDay = dto.TimesOfDay,
                Frequency = dto.Frequency,
                IntervalDays = dto.IntervalDays,
                DaysOfWeek = dto.DaysOfWeek,
                RemindToBuy = dto.RemindToBuy,
                RemindToBuyMethod = dto.RemindToBuyMethod,
                PackQuantity = dto.PackQuantity,
                PillsPerPack = dto.PillsPerPack,
                PillsPerDay = dto.PillsPerDay
            };

            _context.MedicationReminders.Add(reminder);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Напоминание успешно создано", Id = reminder.Id });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateReminder(int id, [FromBody] ReminderCreateDTO dto)
        {
            var userId = GetUserId();
            var reminder = await _context.MedicationReminders
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reminder == null) return NotFound("Напоминание не найдено");

            reminder.ProductId = dto.ProductId;
            reminder.MedicationName = dto.MedicationName;
            reminder.Dosage = dto.Dosage;
            reminder.StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
            reminder.EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);
            reminder.TimesOfDay = dto.TimesOfDay;
            reminder.Frequency = dto.Frequency;
            reminder.IntervalDays = dto.IntervalDays;
            reminder.DaysOfWeek = dto.DaysOfWeek;
            reminder.RemindToBuy = dto.RemindToBuy;
            reminder.RemindToBuyMethod = dto.RemindToBuyMethod;
            reminder.PackQuantity = dto.PackQuantity;
            reminder.PillsPerPack = dto.PillsPerPack;
            reminder.PillsPerDay = dto.PillsPerDay;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Курс успешно обновлен" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReminder(int id)
        {
            var userId = GetUserId();
            var reminder = await _context.MedicationReminders
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reminder == null) return NotFound("Напоминание не найдено");

            _context.MedicationReminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Напоминание удалено" });
        }

        [HttpPost("{id}/take")]
        public async Task<ActionResult> TakeMedication(int id, [FromQuery] DateTime date, [FromQuery] string time)
        {
            var userId = GetUserId();
            var reminder = await _context.MedicationReminders
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reminder == null) return NotFound("Напоминание не найдено");

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var existingLog = await _context.MedicationLogs
                .FirstOrDefaultAsync(l => l.MedicationReminderId == id && l.TakenDate == utcDate && l.ScheduledTime == time);

            if (existingLog != null)
                return BadRequest(new { Message = "Этот прием уже отмечен" });

            var log = new MedicationLog
            {
                MedicationReminderId = id,
                TakenDate = utcDate,
                ScheduledTime = time,
                ActualTakenAt = DateTime.UtcNow
            };

            _context.MedicationLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Прием успешно отмечен" });
        }

        [HttpDelete("{id}/untake")]
        public async Task<ActionResult> UntakeMedication(int id, [FromQuery] DateTime date, [FromQuery] string time)
        {
            var userId = GetUserId();

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var log = await _context.MedicationLogs
                .Include(l => l.MedicationReminder)
                .FirstOrDefaultAsync(l => l.MedicationReminderId == id
                                       && l.MedicationReminder.UserId == userId
                                       && l.TakenDate == utcDate
                                       && l.ScheduledTime == time);

            if (log == null) return NotFound();

            _context.MedicationLogs.Remove(log);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Отметка о приеме отменена" });
        }
    }
}
