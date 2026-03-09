using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Models;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public FavoritesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteDTO>>> GetFavorites()
        {
            var userId = GetUserId();
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .ProjectTo<FavoriteDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(favorites);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToFavorite(int productId)
        {
            var userId = GetUserId();
            var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
            if(exists)
            {
                return Ok();
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromFavorite(int productId)
        {
            var userId = GetUserId();
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            if(favorite == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
