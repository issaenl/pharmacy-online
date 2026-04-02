using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Models;
using pharmacyBackend.Services;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImportService _import;

        public StocksController(AppDbContext context, IMapper mapper, IImportService import)
        {
            _context = context;
            _mapper = mapper;
            _import = import;
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpGet("admin-all")]
        public async Task<ActionResult<IEnumerable<PharmacyStockFullDTO>>> GetAllStocks()
        {
            var query = _context.Stocks
                .Include(s => s.Pharmacy)
                .Include(s => s.Product)
                .AsQueryable();

            var claimId = User.FindFirst("PharmacyId")?.Value;
            if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int pharmacyId))
            {
                query = query.Where(s => s.PharmacyId == pharmacyId);
            }

            var stocks = await query
                .ProjectTo<PharmacyStockFullDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(s => s.LastUpdate)
                .ToListAsync();
            return Ok(stocks);
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpPost]
        public async Task<ActionResult> CreateStock([FromBody] StockCreateDTO dto)
        {
            var claimId = User.FindFirst("PharmacyId")?.Value;
            if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int pharmacyId))
            {
                if (dto.PharmacyId != pharmacyId)
                {
                    return Forbid();
                }
            }

            var existing = await _context.Stocks.FirstOrDefaultAsync(s => s.PharmacyId == dto.PharmacyId && s.ProductId == dto.ProductId);
            if(existing != null)
            {
                return BadRequest(new { Message = "Этот товар уже есть в аптеке" });
            }

            if (!DateOnly.TryParse(dto.ExpirationDate, out var expDate))
            {
                return BadRequest(new { Message = "Неверный формат срока годности." });
            }

            var stock = new Stock
            {
                PharmacyId = dto.PharmacyId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = dto.Price,
                ExpirationDate = expDate,
                LastUpdate = DateTime.UtcNow,
                DiscountPercentage = dto.DiscountPercentage
            };
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();
            return Ok(new {Message = "Товар добавлен в наличие в аптеку" });
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStock(int id, [FromBody] StockCreateDTO dto)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if(stock == null)
            {
                return NotFound();
            }

            var claimId = User.FindFirst("PharmacyId")?.Value;
            if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int pharmacyId))
            {
                if (stock.PharmacyId != pharmacyId || dto.PharmacyId != pharmacyId)
                {
                    return Forbid();
                }
            }

            if (!DateOnly.TryParse(dto.ExpirationDate, out var expDate))
            {
                return BadRequest(new { Message = "Неверный формат срока годности." });
            }

            stock.PharmacyId = dto.PharmacyId;
            stock.ProductId = dto.ProductId;
            stock.Quantity = dto.Quantity;
            stock.Price = dto.Price;
            stock.ExpirationDate = expDate;
            stock.LastUpdate = DateTime.UtcNow;
            stock.DiscountPercentage = dto.DiscountPercentage;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Наличие успешно обновлено" });
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            var claimId = User.FindFirst("PharmacyId")?.Value;
            if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int pharmacyId))
            {
                if (stock.PharmacyId != pharmacyId)
                {
                    return Forbid();
                }
            }

            try
            {
                _context.Stocks.Remove(stock);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Запас успешно удален" });
            }
            catch (DbUpdateException)
            {
                stock.Quantity = 0;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Товар нельзя полностью удалить из базы (он есть в истории заказов), поэтому его остаток обнулен." });
            }
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpPost("import")]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Файл пуст или не выбран." });
            }

            var result = await _import.ImportStocksAsync(file);

            if (result.AddedCount == 0 && result.Errors.Any())
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
