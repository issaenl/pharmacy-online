using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.Hubs;
using pharmacyBackend.Models;

namespace pharmacyBackend.Services
{
    public class WaitlistService : IWaitlistService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public WaitlistService(
            AppDbContext context,
            IEmailService emailService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        public async Task CheckWaitlistOnStockUpdateAsync(int productId, int pharmacyId, int quantity)
        {
            if (quantity <= 0) return;

            var pharmacy = await _context.Pharmacies.FindAsync(pharmacyId);
            var product = await _context.Products.FindAsync(productId);

            if (pharmacy == null || product == null) return;

            var waitlistedUsers = await _context.WaitlistItems
                .Include(w => w.User)
                .Where(w => w.ProductId == productId && w.District == pharmacy.District)
                .ToListAsync();

            if (!waitlistedUsers.Any()) return;

            foreach (var item in waitlistedUsers)
            {
                var message = $"Товар {product.Name} снова в наличии в аптеке {pharmacy.Name} по адресу {pharmacy.Address}";

                if (!string.IsNullOrEmpty(item.User.Email))
                {
                    await _emailService.SendEmailAsync(item.User.Email, "Поступление товара", message);
                }

                var notification = new Notification
                {
                    UserId = item.UserId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _hubContext.Clients.User(item.UserId.ToString()).SendAsync("ReceiveNotification", notification);

                _context.WaitlistItems.Remove(item);
            }

            await _context.SaveChangesAsync();
        }
    }
}
