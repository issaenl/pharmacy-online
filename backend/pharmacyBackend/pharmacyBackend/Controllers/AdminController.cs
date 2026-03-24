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


        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<GeneralAdminDTO>> GetGeneralAdminStatistics()
        {
            var today = DateTime.UtcNow.Date;
            var monthAgo = today.AddMonths(-1);
            var expirationThreshold = today.AddMonths(1);

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == Enums.OrderStatus.Completed)
                .SumAsync(o => o.TotalPrice);
            var ordersToday = await _context.Orders.CountAsync(o => o.OrderDate.Date == today);
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
                .Where(oi => oi.Order.Status == Enums.OrderStatus.Completed && oi.Order.OrderDate >= monthAgo)
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
                OrdersToday = ordersToday,
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
        public async Task<ActionResult<PharmacyAdminDTO>> GetPharmacyAdminStatistics(int pharmacyId)
        {
            var today = DateTime.UtcNow.Date;
            var monthAgo = today.AddMonths(-1);
            var expirationThreshold = today.AddMonths(1);

            var revenueToday = await _context.Orders
                .Where(o => o.Status == Enums.OrderStatus.Completed && o.PharmacyId == pharmacyId)
                .SumAsync(o => o.TotalPrice);
            var pendingOrders = await _context.Orders.CountAsync(o => o.PharmacyId == pharmacyId && o.Status == OrderStatus.Pending);
            var completedToday = await _context.Orders.CountAsync(o => o.PharmacyId == pharmacyId && o.Status == OrderStatus.Completed && o.OrderDate.Date == today);
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
                .Where(oi => oi.Order.PharmacyId == pharmacyId && oi.Order.Status == OrderStatus.Completed && oi.Order.OrderDate >= monthAgo)
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
                RevenueToday = revenueToday,
                PendingOrders = pendingOrders,
                CompletedTodayOrders = completedToday,
                LowStockCount = lowStockCount,
                LowStockProducts = lowStockProducts,
                TopProducts = topProducts,
                ExpiringProducts = expiringProductsLocal
            };
            return Ok(stats);
        }
    }
}
