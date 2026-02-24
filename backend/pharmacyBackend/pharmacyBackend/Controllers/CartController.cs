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
    public class CartController : BaseController
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Stocks)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            var cart = await GetOrCreateCartAsync(GetUserId());

            var cartDto = new CartDTO
            {
                PharmacyId = cart.PharmacyId,
                Items = cart.CartItems.Select(ci => new CartItemDTO
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.Product.Stocks.Any() ? ci.Product.Stocks.Min(s => s.Price) : 0,
                    PictureUrl = ci.Product.PictureUrl,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return Ok(cartDto);
        }

        [HttpPost("sync")]
        public async Task<ActionResult> SyncCart([FromBody] List<AddToCartDTO> localItems)
        {
            var cart = await GetOrCreateCartAsync(GetUserId());

            foreach (var localItem in localItems)
            {
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == localItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity = Math.Max(existingItem.Quantity, localItem.Quantity);
                }
                else
                {
                    cart.CartItems.Add(new CartItem { ProductId = localItem.ProductId, Quantity = localItem.Quantity });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDTO request)
        {
            var cart = await GetOrCreateCartAsync(GetUserId());
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (existingItem != null)
                existingItem.Quantity += request.Quantity;
            else
                cart.CartItems.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("update")]
        public async Task<ActionResult> UpdateQuantity([FromBody] AddToCartDTO request)
        {
            var cart = await GetOrCreateCartAsync(GetUserId());
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (item != null)
            {
                item.Quantity = request.Quantity;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpDelete("remove/{productId}")]
        public async Task<ActionResult> RemoveItem(int productId)
        {
            var cart = await GetOrCreateCartAsync(GetUserId());
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            var cart = await GetOrCreateCartAsync(GetUserId());
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
