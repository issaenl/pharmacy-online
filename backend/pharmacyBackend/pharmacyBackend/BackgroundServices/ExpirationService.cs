using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.Services;

namespace pharmacyBackend.Background_Services
{
    public class ExpirationService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ExpirationService> _logger;

        public ExpirationService(IServiceProvider provider, ILogger<ExpirationService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using(var scope = _provider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var generator = scope.ServiceProvider.GetRequiredService<IEmailGenerator>();
                        var expirationTime = DateTime.UtcNow.AddHours(-48);
                        var expiredOrders = await context.Orders
                            .Include(o => o.User)
                            .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Stocks)
                            .Where(o => o.Status == Enums.OrderStatus.Ready && o.ReadyDate != null && o.ReadyDate < expirationTime)
                            .ToListAsync(stoppingToken);

                        foreach(var order in expiredOrders)
                        {
                            order.Status = Enums.OrderStatus.Expired;
                            foreach(var item in order.OrderItems)
                            {
                                var stock = item.Product.Stocks.FirstOrDefault(s => s.PharmacyId == order.PharmacyId);
                                if(stock != null)
                                {
                                    stock.Quantity += item.Quantity;
                                }
                            }

                            if(order.User != null && !string.IsNullOrEmpty(order.User.Email))
                            {
                                var (subject, htmlBody) = generator.GenerateStatusEmail(order.User, order, Enums.OrderStatus.Expired);
                                if(!string.IsNullOrEmpty(subject))
                                {
                                    await emailService.SendEmailAsync(order.User.Email, subject, htmlBody);
                                }
                            }
                        }

                        if(expiredOrders.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"{expiredOrders.Count} заказов истекли");
                        }
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при проверке срока заказов");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
