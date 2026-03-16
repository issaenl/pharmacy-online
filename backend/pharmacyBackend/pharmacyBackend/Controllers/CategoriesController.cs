using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImportService _import;

        public CategoriesController(AppDbContext context, IMapper mapper, IImportService import)
        {
            _context = context;
            _mapper = mapper;
            _import = import;
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDTO)
        {
            var category = new Category
            {
                Name = categoryDTO.Name,
                Description = categoryDTO.Description,
                CategoryTags = new List<CategoryTag>()
            };

            await ProcessTags(category, categoryDTO.Tags);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok( new { Message = "Категория создана" });

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(int id, [FromBody] CategoryCreateDTO categoryDto)
        {
            var category = await _context.Categories
                .Include(c => c.CategoryTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            _context.CategoryTags.RemoveRange(category.CategoryTags);
            await ProcessTags(category, categoryDto.Tags);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Категория обновлена"});
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Категория удалена" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { Message = "Невозможно удалить категорию, так как в ней есть товары." });
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

            var result = await _import.ImportCategoriesAsync(file);

            if (result.AddedCount == 0 && result.Errors.Any())
            {
                return BadRequest(result);
            }

            return Ok(result);

        }


        private async Task ProcessTags(Category category, List<string> tagNames)
        {
            if(tagNames == null || !tagNames.Any())
            {
                return;
            }

            foreach (var tag in tagNames)
            {
                var cleanTag = tag.Trim().ToLower();
                if(string.IsNullOrWhiteSpace(cleanTag)) continue;
                var existTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == cleanTag);
                if(existTag == null)
                {
                    existTag = new Tag
                    {
                        Name = cleanTag,
                    };
                    _context.Tags.Add(existTag);
                    await _context.SaveChangesAsync();
                    
                }

                category.CategoryTags.Add(new CategoryTag { CategoryId = category.Id, TagId = existTag.Id });
            }
        }
    }
}
