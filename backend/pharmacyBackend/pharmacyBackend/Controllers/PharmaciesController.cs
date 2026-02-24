using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using Microsoft.EntityFrameworkCore;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmaciesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PharmaciesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PharmacyFullDTO>>> GetAllPharmacies()
        {
            var pharmacies = await _context.Pharmacies
                .ProjectTo<PharmacyFullDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(pharmacies);
        }

        [HttpPost("available-for-cart")]
        public async Task<ActionResult<IEnumerable<PharmacyFullDTO>>> GetAvailablePharmacies([FromBody] List<AddToCartDTO> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
            {
                return BadRequest("Корзина пуста.");
            }

            var query = _context.Pharmacies.AsQueryable();

            foreach (var item in cartItems)
            {
                query = query.Where(p => p.Stocks.Any(s =>
                    s.ProductId == item.ProductId &&
                    s.Quantity >= item.Quantity)); 
            }

            var availablePharmacies = await query
                .ProjectTo<PharmacyFullDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(availablePharmacies);
        }
    }
}
