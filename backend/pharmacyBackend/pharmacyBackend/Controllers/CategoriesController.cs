using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.CategoryTags)
                    .ThenInclude(ct => ct.Tag)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryDTO>>(categories));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.CategoryTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return Ok(_mapper.Map<CategoryDTO>(category));
        }
    }
}
