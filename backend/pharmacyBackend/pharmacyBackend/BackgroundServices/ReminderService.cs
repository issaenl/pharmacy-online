using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.Enums;
using pharmacyBackend.Models;
using pharmacyBackend.Services;
using Microsoft.AspNetCore.SignalR;
using pharmacyBackend.Hubs;

namespace pharmacyBackend.BackgroundServices
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public ReminderService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                    var nowUtc = DateTime.UtcNow;
                    var nowLocal = DateTime.Now;
                    var currentTime = nowLocal.ToString("HH:mm");

                    var reminders = await context.MedicationReminders
                        .Include(r => r.User)
                        .Where(r => r.StartDate <= nowUtc.Date && r.EndDate >= nowUtc.Date)
                        .ToListAsync(stoppingToken);

                    foreach (var r in reminders)
                    {
                        bool isDueToday = false;
                        if (r.Frequency == MedicationFrequency.Daily) isDueToday = true;
                        else if (r.Frequency == MedicationFrequency.EveryXDays && r.IntervalDays.HasValue)
                        {
                            var diffDays = (nowUtc.Date - r.StartDate.Date).Days;
                            if (diffDays % r.IntervalDays.Value == 0) isDueToday = true;
                        }
                        else if (r.Frequency == MedicationFrequency.SpecificDaysOfWeek && !string.IsNullOrEmpty(r.DaysOfWeek))
                        {
                            if (r.DaysOfWeek.Contains(nowLocal.DayOfWeek.ToString())) isDueToday = true;
                        }

                        if (isDueToday && r.TimesOfDay.Contains(currentTime))
                        {
                            await SendNotification(context, hubContext, emailService, r,
                                $"Пора принять лекарство: {r.MedicationName} ({r.Dosage})", stoppingToken);
                        }

                        if (r.RemindToBuy && currentTime == "10:00")
                        {
                            DateTime runOutDate = r.EndDate.Date;
                            bool skipReminder = false;
                            if (r.RemindToBuyMethod == 2 && r.PackQuantity.HasValue && r.PillsPerPack.HasValue && r.PillsPerDay.HasValue && r.PillsPerDay > 0)
                            {
                                var totalPills = r.PackQuantity.Value * r.PillsPerPack.Value;
                                var daysItLasts = (int)(totalPills / r.PillsPerDay.Value);

                                runOutDate = r.StartDate.Date.AddDays(daysItLasts);

                                if (runOutDate >= r.EndDate.Date)
                                {
                                    skipReminder = true;
                                }
                            }

                            if (!skipReminder)
                            {
                                var daysUntilEnd = (runOutDate - nowUtc.Date).Days;

                                if (daysUntilEnd == 3)
                                {
                                    var buyMsg = $"Внимание! Лекарство '{r.MedicationName}' скоро закончится (через 3 дня). Не забудьте купить новую упаковку.";
                                    await SendNotification(context, hubContext, emailService, r, buyMsg, stoppingToken);
                                }
                            }
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }

                var secondsToNextMinute = 60 - DateTime.Now.Second;
                await Task.Delay(TimeSpan.FromSeconds(secondsToNextMinute), stoppingToken);
            }
        }
        private async Task SendNotification(AppDbContext context, IHubContext<NotificationHub> hubContext,
            IEmailService emailService, MedicationReminder r, string message, CancellationToken ct)
        {
            var newNotification = new Notification
            {
                UserId = r.UserId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            context.Notifications.Add(newNotification);
            await context.SaveChangesAsync(ct);

            await hubContext.Clients.User(r.UserId.ToString())
                .SendAsync("ReceiveNotification", newNotification, cancellationToken: ct);

            if (r.User != null && !string.IsNullOrEmpty(r.User.Email))
            {
                _ = emailService.SendEmailAsync(r.User.Email, "Уведомление от Аптеки", message);
            }
        }
    }
}