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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-all")]
        public async Task<ActionResult<IEnumerable<PharmacyStockFullDTO>>> GetAllStocks()
        {
            var stocks = await _context.Stocks
                .Include(s => s.Pharmacy)
                .Include(s => s.Product)
                .ProjectTo<PharmacyStockFullDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(s => s.LastUpdate)
                .ToListAsync();
            return Ok(stocks);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateStock([FromBody] StockCreateDTO dto)
        {
            var existing = await _context.Stocks.FirstOrDefaultAsync(s => s.PharmacyId == dto.PharmacyId && s.ProductId == dto.ProductId);
            if(existing != null)
            {
                return BadRequest(new { Message = "Этот товар уже есть в аптеке" });
            }

            var stock = new Stock
            {
                PharmacyId = dto.PharmacyId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = dto.Price,
                LastUpdate = DateTime.UtcNow
            };
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();
            return Ok(new {Message = "Товар добавлен в наличие в аптеку" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStock(int id, [FromBody] StockCreateDTO dto)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if(stock == null)
            {
                return NotFound();
            }

            stock.PharmacyId = dto.PharmacyId;
            stock.ProductId = dto.ProductId;
            stock.Quantity = dto.Quantity;
            stock.Price = dto.Price;
            stock.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Наличие успешно обновлено" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
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

        [Authorize(Roles = "Admin")]
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
