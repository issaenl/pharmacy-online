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
    public class PharmaciesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly GeocodeService _geocoder;

        public PharmaciesController(AppDbContext context, IMapper mapper, GeocodeService geocodeService)
        {
            _context = context;
            _mapper = mapper;
            _geocoder = geocodeService;
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var item in cartItems)
            {
                query = query.Where(p => p.Stocks.Any(s =>
                    s.ProductId == item.ProductId &&
                    s.Quantity >= item.Quantity &&
                    s.ExpirationDate > today));
            }

            var availablePharmacies = await query
                .ProjectTo<PharmacyFullDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(availablePharmacies);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PharmacyFullDTO>> CreatePharmacy([FromBody] PharmacyCreateDTO pharma)
        {
            double finalLat;
            double finalLon;

            if (!pharma.Latitude.HasValue || !pharma.Longitude.HasValue)
            {
                var addressForGeocode = $"{pharma.District}, {pharma.Address}";
                var (lat, lon) = await _geocoder.GetCoordinatesAync(addressForGeocode);

                if (!lat.HasValue || !lon.HasValue)
                {
                    return BadRequest("Не удалось автоматически определить координаты по адресу. Пожалуйста, укажите Latitude и Longitude вручную.");
                }

                finalLat = lat.Value;
                finalLon = lon.Value;
            }
            else
            {
                finalLat = pharma.Latitude.Value;
                finalLon = pharma.Longitude.Value;
            }

            var pharmacy = _mapper.Map<Pharmacy>(pharma);

            pharmacy.Latitude = finalLat;
            pharmacy.Longitude = finalLon;

            _context.Pharmacies.Add(pharmacy);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<PharmacyFullDTO>(pharmacy);
            return Ok(resultDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePharmacy(int id, [FromBody] PharmacyCreateDTO pharma)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if(pharmacy == null)
            {
                return NotFound();
            }

            if (!pharma.Latitude.HasValue || !pharma.Longitude.HasValue)
            {
                var addressForGeocode = $"{pharma.District}, {pharma.Address}";
                var (lat, lon) = await _geocoder.GetCoordinatesAync(addressForGeocode);

                if (lat.HasValue && lon.HasValue)
                {
                    pharmacy.Latitude = lat.Value;
                    pharmacy.Longitude = lon.Value;
                }
            }
            else
            {
                pharmacy.Latitude = pharma.Latitude.Value;
                pharmacy.Longitude = pharma.Longitude.Value;
            }

            pharmacy.Name = pharma.Name;
            pharmacy.Address = pharma.Address;
            pharmacy.District = pharma.District;
            pharmacy.Phone = pharma.Phone;
            pharmacy.Rating = pharma.Rating;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Аптека успешно обновлена" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy == null)
            {
                return NotFound();
            }

            _context.Pharmacies.Remove(pharmacy);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Аптека удалена" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PharmacyFullDTO>> GetPharmacyById(int id)
        {
            var pharmacy = await _context.Pharmacies
                .ProjectTo<PharmacyFullDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null)
            {
                return NotFound();
            }
            return Ok(pharmacy);
        }

        [HttpGet("{id}/assortiment")]
        public async Task<ActionResult<IEnumerable<PharmacyAssortmentDTO>>> GetPharmacyAssortiment(int id)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var assortiment = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.PharmacyId == id && s.Quantity > 0 && s.ExpirationDate > today)
                .Select(s => new PharmacyAssortmentDTO
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity,
                    DosageForm = s.Product.DosageForm,
                    Manufacturer = s.Product.Manufacturer,
                    Country = s.Product.Country,
                    Price = s.Price,
                    PictureUrl = s.Product.PictureUrl,
                    ExpirationDate = s.ExpirationDate,
                    LastUpdate = s.LastUpdate
                })
                .OrderBy(s => s.ProductName)
                .ToListAsync();
            return Ok(assortiment);
        }
    }
}
