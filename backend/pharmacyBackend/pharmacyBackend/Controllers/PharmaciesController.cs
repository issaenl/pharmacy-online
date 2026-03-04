using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    }
}
