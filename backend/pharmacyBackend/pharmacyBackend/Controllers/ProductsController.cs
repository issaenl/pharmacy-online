using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Helpers;

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

        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<ProductShortDTO>>> GetProducts([FromQuery] string? categoryIds)
        // {
        //     var query = _context.Products.AsQueryable();

        //     if (!string.IsNullOrWhiteSpace(categoryIds))
        //     {
        //         var ids = categoryIds.Split(',')
        //                              .Select(int.Parse)
        //                              .ToList();

        //         query = query.Where(p => ids.Contains(p.CategoryId));
        //     }

        //     var products = await query
        //         .Include(p => p.Stocks)
        //         .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
        //         .ToListAsync();

        //     return Ok(products);
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductShortDTO>>> GetProducts([FromQuery] ProductFilterParams filters)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.CategoryIds))
            {
                var ids = filters.CategoryIds.Split(',').Select(int.Parse).ToList();
                query = query.Where(p => ids.Contains(p.CategoryId));
            }

            if (filters.IsPrescription.HasValue)
            {
                query = query.Where(p => p.IsPrescription == filters.IsPrescription.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Country))
            {
                query = query.Where(p => p.Country.ToLower() == filters.Country.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filters.Manufacturer))
            {
                query = query.Where(p => p.Manufacturer.ToLower() == filters.Manufacturer.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filters.District))
            {
                query = query.Where(p => p.Stocks.Any(s => 
                    s.Pharmacy.District.ToLower() == filters.District.ToLower() && s.Quantity > 0));
            }

            if (filters.PriceMin.HasValue)
            {
                query = query.Where(p => p.Stocks.Any(s => s.Quantity > 0 && s.Price >= filters.PriceMin.Value));
            }

            if (filters.PriceMax.HasValue)
            {
                query = query.Where(p => p.Stocks.Any(s => s.Quantity > 0 && s.Price <= filters.PriceMax.Value));
            }

            var products = await query
                .Include(p => p.Stocks)
                .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(products);
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
                return NotFound(new { Message = "Товар временно отсутствует" });
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
                return NotFound(new { Message = "Популярные товары не найдены" });
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
                return NotFound(new { Message = "Новые товары не найдены" });
            }

            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductShortDTO>>> SearchProducts([FromQuery] string query)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<ProductShortDTO>());
            }

            string originalQuery = query.Trim().ToLower();
            string otherLayoutQuery = SearchHelper.ConvertLayout(query);

            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(originalQuery) || p.Name.ToLower().Contains(otherLayoutQuery))
                .Include(p => p.Stocks)
                .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!products.Any())
            {
                var allProducts = await _context.Products
                    .Select(p => new { p.Id, Name = p.Name.ToLower()})
                    .ToListAsync();

                var matches = allProducts
                    .Select(p => new
                    {
                        p.Id,
                        Distance = Math.Min(SearchHelper.LevensheteinAlgorithm(originalQuery, p.Name),
                    SearchHelper.LevensheteinAlgorithm(otherLayoutQuery, p.Name))
                    })
                    .Where(x => x.Distance <= (originalQuery.Length > 4 ? 4 : 2))
                    .OrderBy(x => x.Distance)
                    .Take(10)
                    .Select(x => x.Id)
                    .ToList();

                products = await _context.Products
                    .Where(p => matches.Contains(p.Id))
                    .Include (p => p.Stocks)
                    .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                products = products.OrderBy(p => matches.IndexOf(p.Id)).ToList();
            }

            return Ok(products);
        }
    }
}
