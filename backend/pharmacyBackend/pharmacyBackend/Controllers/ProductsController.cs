using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Helpers;
using pharmacyBackend.Models;
using pharmacyBackend.Services;
using System.Globalization;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICloudService _cloud;
        private readonly IImportService _import;

        public ProductsController(AppDbContext context, IMapper mapper, ICloudService cloud, IImportService importService)
        {
            _context = context;
            _mapper = mapper;
            _cloud = cloud;
            _import = importService;
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
                .Where(p => p.IsActive == true)
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var avalibility = await _context.Stocks
                .Where(s => s.ProductId == id && s.Quantity > 0 && s.ExpirationDate > today)
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
                .Where(p => p.IsActive == true)
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
                .Where(p => p.IsActive == true)
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
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<ProductShortDTO>());
            }

            string originalQuery = query.Trim().ToLower();
            string otherLayoutQuery = SearchHelper.ConvertLayout(query);

            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(originalQuery) || p.Name.ToLower().Contains(otherLayoutQuery) && p.IsActive == true)
                .Include(p => p.Stocks)
                .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!products.Any())
            {
                var allProducts = await _context.Products
                    .Where(p => p.IsActive == true)
                    .Select(p => new
                    {
                        p.Id,
                        Name = p.Name.ToLower(),
                        Tags = p.Category.CategoryTags.Select(ct => ct.Tag.Name.ToLower()).ToList()
                    })
                    .ToListAsync();

                var matches = allProducts
                    .Select(p =>
                    {
                        int nameDistance = Math.Min(
                            SearchHelper.LevensheteinAlgorithm(originalQuery, p.Name),
                            SearchHelper.LevensheteinAlgorithm(otherLayoutQuery, p.Name)
                        );

                        int tagDistance = int.MaxValue;
                        if (p.Tags.Any())
                        {
                            tagDistance = p.Tags.Min(t => Math.Min(
                                SearchHelper.LevensheteinAlgorithm(originalQuery, t),
                                SearchHelper.LevensheteinAlgorithm(otherLayoutQuery, t)
                            ));
                        }

                        return new
                        {
                            p.Id,
                            Distance = Math.Min(nameDistance, tagDistance)
                        };
                    })
                    .Where(x => x.Distance <= (originalQuery.Length > 4 ? 4 : 2))
                    .OrderBy(x => x.Distance)
                    .Take(10)
                    .Select(x => x.Id)
                    .ToList();

                products = await _context.Products
                    .Where(p => matches.Contains(p.Id))
                    .Include(p => p.Stocks)
                    .ProjectTo<ProductShortDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                products = products.OrderBy(p => matches.IndexOf(p.Id)).ToList();
            }

            return Ok(products);
        }

        [Authorize(Roles = "Admin, PharmacyAdmin")]
        [HttpGet("admin-all-products")]
        public async Task<ActionResult<IEnumerable<ProductFullDTO>>> GetAllForAdmins()
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .ProjectTo<ProductFullDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateProduct([FromForm] ProductUploadDTO upload)
        {
            var product = new Product
            {
                Name = upload.Name,
                Manufacturer = upload.Manufacturer,
                Country = upload.Country,
                IsPrescription = upload.IsPrescription,
                DosageForm = upload.DosageForm,
                CategoryId = upload.CategoryId,
                IsActive = upload.IsActive,
            };

            if (upload.PictureFile != null)
            {
                product.PictureUrl = await _cloud.UploadFileAsync(upload.PictureFile);
            }

            if (upload.PdfFile != null)
            {
                product.PdfUrl = await _cloud.UploadFileAsync(upload.PdfFile);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Продукт успешно создан!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromForm] ProductUploadDTO upload)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = upload.Name;
            product.Manufacturer = upload.Manufacturer;
            product.Country = upload.Country;
            product.IsPrescription = upload.IsPrescription;
            product.DosageForm = upload.DosageForm;
            product.CategoryId = upload.CategoryId;
            product.IsActive = upload.IsActive;

            if (upload.PictureFile != null)
            {
                product.PictureUrl = await _cloud.UploadFileAsync(upload.PictureFile);
            }

            if (upload.PdfFile != null)
            {
                product.PdfUrl = await _cloud.UploadFileAsync(upload.PdfFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Продукт обновлен" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Продукт удален" });

        }


        [Authorize(Roles = "Admin")]
        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Файл пуст или не выбран." });
            }

            var result = await _import.ImportProductsAsync(file);

            if (result.AddedCount == 0 && result.Errors.Any())
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
