using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;
using ClosedXML.Excel;
using System.IO;

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

        [Authorize(Roles = "PharmacyAdmin, Admin")]
        [HttpGet("pharmacy/{pharmacyId}/export")]
        public async Task<IActionResult> ExportPharmacyReport(int pharmacyId, [FromQuery] string period = "today")
        {
            var startDate = GetStartDate(period);
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var stocks = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.PharmacyId == pharmacyId)
                .OrderBy(s => s.Product.Name)
                .ToListAsync();

            var sales = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.PharmacyId == pharmacyId
                          && oi.Order.Status == OrderStatus.Completed
                          && oi.Order.OrderDate >= startDate)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Price)
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var wsStock = workbook.Worksheets.Add("Весь склад");
            wsStock.Cell(1, 1).Value = "Название препарата";
            wsStock.Cell(1, 2).Value = "Остаток (шт)";
            wsStock.Cell(1, 3).Value = "Срок годности";
            wsStock.Cell(1, 4).Value = "Статус";
            wsStock.Range("A1:D1").Style.Font.Bold = true;

            int row = 2;
            foreach (var item in stocks)
            {
                wsStock.Cell(row, 1).Value = item.Product.Name;
                wsStock.Cell(row, 2).Value = item.Quantity;
                wsStock.Cell(row, 3).Value = item.ExpirationDate.ToString("dd.MM.yyyy");

                string status = "В норме";
                if (item.Quantity == 0) status = "Нет в наличии";
                else if (item.ExpirationDate < today) status = "Просрочено";
                else if (item.ExpirationDate <= today.AddMonths(1)) status = "Истекает срок";

                wsStock.Cell(row, 4).Value = status;

                if (item.Quantity == 0)
                {
                    wsStock.Row(row).Style.Font.FontColor = XLColor.Red;
                }

                row++;
            }
            wsStock.Columns().AdjustToContents();

            var wsLowStock = workbook.Worksheets.Add("Дефицит (нужно заказать)");
            wsLowStock.Cell(1, 1).Value = "Название препарата";
            wsLowStock.Cell(1, 2).Value = "Остаток (шт)";
            wsLowStock.Range("A1:B1").Style.Font.Bold = true;

            int lowRow = 2;
            var lowStockItems = stocks.Where(s => s.Quantity < 5).OrderBy(s => s.Quantity).ToList();

            foreach (var item in lowStockItems)
            {
                wsLowStock.Cell(lowRow, 1).Value = item.Product.Name;
                wsLowStock.Cell(lowRow, 2).Value = item.Quantity;

                if (item.Quantity == 0)
                {
                    wsLowStock.Row(lowRow).Style.Font.FontColor = XLColor.Red;
                }

                lowRow++;
            }
            wsLowStock.Columns().AdjustToContents();

            var wsSales = workbook.Worksheets.Add("Продажи за период");
            wsSales.Cell(1, 1).Value = "Название препарата";
            wsSales.Cell(1, 2).Value = "Продано (шт)";
            wsSales.Cell(1, 3).Value = "Выручка";
            wsSales.Range("A1:C1").Style.Font.Bold = true;

            int sRow = 2;
            foreach (var sale in sales)
            {
                wsSales.Cell(sRow, 1).Value = sale.ProductName;
                wsSales.Cell(sRow, 2).Value = sale.TotalQuantity;
                wsSales.Cell(sRow, 3).Value = sale.TotalRevenue;
                sRow++;
            }
            wsSales.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string fileName = $"Pharmacy_{pharmacyId}_Report_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
