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

                        if (r.Frequency == MedicationFrequency.Daily)
                        {
                            isDueToday = true;
                        }
                        else if (r.Frequency == MedicationFrequency.EveryXDays && r.IntervalDays.HasValue)
                        {
                            var diffDays = (nowUtc.Date - r.StartDate.Date).Days;
                            if (diffDays % r.IntervalDays.Value == 0)
                            {
                                isDueToday = true;
                            }
                        }
                        else if (r.Frequency == MedicationFrequency.SpecificDaysOfWeek && !string.IsNullOrEmpty(r.DaysOfWeek))
                        {
                            var currentDay = nowLocal.DayOfWeek.ToString();
                            if (r.DaysOfWeek.Contains(currentDay))
                            {
                                isDueToday = true;
                            }
                        }

                        if (isDueToday && r.TimesOfDay.Contains(currentTime))
                        {
                            var msg = $"Пора принять лекарство: {r.MedicationName} ({r.Dosage})";

                            var newNotification = new Notification
                            {
                                UserId = r.UserId,
                                Message = msg,
                                CreatedAt = DateTime.UtcNow,
                                IsRead = false
                            };

                            context.Notifications.Add(newNotification);
                            await context.SaveChangesAsync(stoppingToken);

                            await hubContext.Clients.User(r.UserId.ToString())
                                .SendAsync("ReceiveNotification", newNotification, cancellationToken: stoppingToken);

                            if (r.User != null && !string.IsNullOrEmpty(r.User.Email))
                            {
                                _ = emailService.SendEmailAsync(r.User.Email, "Напоминание о приеме лекарств", msg);
                            }
                        }
                    }
                    await context.SaveChangesAsync(stoppingToken);
                }

                var secondsToNextMinute = 60 - DateTime.Now.Second;
                await Task.Delay(TimeSpan.FromSeconds(secondsToNextMinute), stoppingToken);
            }
        }
    }
}