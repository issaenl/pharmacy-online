using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.DTO;
using pharmacyBackend.Models;
using System.Security.Claims;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WaitlistController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WaitlistController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetWaitlist()
        {
            var items = await _context.WaitlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == GetUserId())
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            var itemsDto = _mapper.Map<List<WaitlistItemDTO>>(items);
            return Ok(itemsDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWaitlist([FromBody] AddWaitlistDto dto)
        {
            var userId = GetUserId();
            var normalizedDistrict = dto.District.Trim().ToLower();

            var exists = await _context.WaitlistItems.AnyAsync(w =>
                w.UserId == userId &&
                w.ProductId == dto.ProductId &&
                w.District.ToLower() == normalizedDistrict);

            if (exists)
            {
                return BadRequest(new { message = "Этот товар уже отслеживается вами в данном регионе." });
            }

            var item = new WaitlistItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                District = dto.District.Trim()
            };

            _context.WaitlistItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWaitlist(int id)
        {
            var item = await _context.WaitlistItems.FirstOrDefaultAsync(w => w.Id == id && w.UserId == GetUserId());
            if (item == null) return NotFound();

            _context.WaitlistItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearWaitlist()
        {
            var items = await _context.WaitlistItems.Where(w => w.UserId == GetUserId()).ToListAsync();
            _context.WaitlistItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
