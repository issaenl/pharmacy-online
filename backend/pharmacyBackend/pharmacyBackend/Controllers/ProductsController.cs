using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductShortDTO>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Stocks)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ProductShortDTO>>(products));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductLongDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Pharmacy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductLongDTO>(product));
        }

        [HttpGet("{id}/availibility")]
        public async Task<ActionResult<IEnumerable<PharmacyStockDTO>>> GetProductAvalibility(int id)
        {
            var avalibility = await _context.Stocks
                .Where(s => s.ProductId == id && s.Quantity > 0)
                .Include(s => s.Pharmacy)
                .ProjectTo<PharmacyStockDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!avalibility.Any())
            {
                return NotFound(new {Message = "Товар временно отсутствует"});
            }

            return Ok(avalibility);
        }

        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<ProductShortDTO>>> GetPopularProducts()
        {
            var products = await _context.Products
                .Include(p => p.Stocks)
                .OrderByDescending(p => p.Stocks.Count(s => s.Quantity > 0))
                .Take(20)
                .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!products.Any()) {
                return NotFound(new { Message = "Популярные товары не найдены"});
            }

            return Ok(products);
        }

        [HttpGet("new")]
        public async Task<ActionResult<IEnumerable<ProductShortDTO>>> GetNewProducts()
        {
            var products = await _context.Products
                .Include(p => p.Stocks)
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!products.Any())
            {
                return NotFound(new { Message = "Новые товары не найдены"});
            }

            return Ok(products);
        }
    }
}
