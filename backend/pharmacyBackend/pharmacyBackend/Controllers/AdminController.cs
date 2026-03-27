using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }


        private DateTime GetStartDate(string period)
        {
            var today = DateTime.UtcNow.Date;
            return period?.ToLower() switch
            {
                "week" => today.AddDays(-7),
                "month" => today.AddMonths(-1),
                "year" => today.AddYears(-1),
                "all" => DateTime.MinValue,
                _ => today
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<GeneralAdminDTO>> GetGeneralAdminStatistics([FromQuery] string period = "today")
        {
            var startDate = GetStartDate(period);
            var expirationThreshold = DateTime.UtcNow.Date.AddMonths(1);
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= startDate)
                .SumAsync(o => o.TotalPrice);

            var ordersPeriod = await _context.Orders.CountAsync(o => o.OrderDate >= startDate);
            var activePharmacies = await _context.Pharmacies.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var lowStockCount = await _context.Stocks.CountAsync(s => s.Quantity < 5);
            var lowStockProducts = await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Pharmacy)
                .Where(s => s.Quantity < 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .Select(s => new LowStockProductDTO
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity,
                    PharmacyName = s.Pharmacy.Name
                })
                .ToListAsync();
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed && oi.Order.OrderDate >= startDate)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(o => o.TotalSold)
                .Take(5)
                .ToListAsync();
            var expiringProducts = await _context.Stocks
                 .Include(s => s.Product)
                 .Include(s => s.Pharmacy)
                 .Where(s => s.Quantity > 0 && s.ExpirationDate <= DateOnly.FromDateTime(expirationThreshold))
                 .Select(s => new ExpiringProductDTO
                 {
                     ProductId = s.ProductId,
                     ProductName = s.Product.Name,
                     ExpirationDate = s.ExpirationDate,
                     PharmacyName = s.Pharmacy.Name
                 })
                 .OrderBy(ep => ep.ExpirationDate)
                 .Take(10)
                 .ToListAsync();
            var stats = new GeneralAdminDTO
            {
                TotalRevenue = totalRevenue,
                OrdersToday = ordersPeriod,
                ActivePharmacies = activePharmacies,
                TotalProducts = totalProducts,
                GlobalLowStockCount = lowStockCount,
                LowStockProducts = lowStockProducts,
                TopProducts = topProducts,
                ExpiringProducts = expiringProducts
            };
            return Ok(stats);
        }

        [Authorize(Roles = "PharmacyAdmin")]
        [HttpGet("pharmacy/{pharmacyId}")]
        public async Task<ActionResult<PharmacyAdminDTO>> GetPharmacyAdminStatistics(int pharmacyId, [FromQuery] string period = "today")
        {
            var startDate = GetStartDate(period);
            var expirationThreshold = DateTime.UtcNow.Date.AddMonths(1);
            var revenuePeriod = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.PharmacyId == pharmacyId && o.OrderDate >= startDate)
                .SumAsync(o => o.TotalPrice);
            var pendingOrders = await _context.Orders.CountAsync(o => o.PharmacyId == pharmacyId && o.Status == OrderStatus.Pending);
            var completedPeriod = await _context.Orders.CountAsync(o => o.PharmacyId == pharmacyId && o.Status == OrderStatus.Completed && o.OrderDate >= startDate);
            var lowStockCount = await _context.Stocks.CountAsync(s => s.PharmacyId == pharmacyId && s.Quantity < 5);
            var lowStockProducts = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.PharmacyId == pharmacyId && s.Quantity < 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .Select(s => new LowStockProductDTO
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity
                })
                .ToListAsync();
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.PharmacyId == pharmacyId && oi.Order.Status == OrderStatus.Completed && oi.Order.OrderDate >= startDate)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(tp => tp.TotalSold)
                .Take(5)
                .ToListAsync();
            var expiringProductsLocal = await _context.Stocks
                 .Include(s => s.Product)
                 .Where(s => s.PharmacyId == pharmacyId && s.Quantity > 0 && s.ExpirationDate <= DateOnly.FromDateTime(expirationThreshold))
                 .Select(s => new ExpiringProductDTO
                 {
                     ProductId = s.ProductId,
                     ProductName = s.Product.Name,
                     ExpirationDate = s.ExpirationDate
                 })
                 .OrderBy(ep => ep.ExpirationDate)
                 .Take(10)
                 .ToListAsync();
            var stats = new PharmacyAdminDTO
            {
                RevenueToday = revenuePeriod,
                PendingOrders = pendingOrders,
                CompletedTodayOrders = completedPeriod,
                LowStockCount = lowStockCount,
                LowStockProducts = lowStockProducts,
                TopProducts = topProducts,
                ExpiringProducts = expiringProductsLocal
            };
            return Ok(stats);
        }
    }
}
