using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : Controller
    {
        private readonly AppDbContext _context;

        public FilterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<string>>> GetCountries()
        {
            var countries = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Country))
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(countries);
        }

        [HttpGet("manufacturers")]
        public async Task<ActionResult<IEnumerable<string>>> GetManufacturers()
        {
            var manufacturers = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Manufacturer))
                .Select(p => p.Manufacturer)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Ok(manufacturers);
        }

        [HttpGet("districts")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistricts()
        {
            var districts = await _context.Pharmacies
                .Where(p => !string.IsNullOrEmpty(p.District))
                .Select(p => p.District)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return Ok(districts);
        }
    }
}
