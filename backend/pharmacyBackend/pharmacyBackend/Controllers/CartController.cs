using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            PharmacyDTO? pharmacyDto = null;
            if (cart.PharmacyId.HasValue)
            {
                var p = await _context.Pharmacies.FindAsync(cart.PharmacyId.Value);
                if (p != null)
                {
                    pharmacyDto = new PharmacyDTO { Id = p.Id, Name = p.Name, Address = p.Address, District = p.District };
                }
            }

            var cartDto = new CartDTO
            {
                PharmacyId = cart.PharmacyId,
                Pharmacy = pharmacyDto,
                Items = cart.CartItems.Select(ci =>
                {
                    var activeStocks = ci.Product.Stocks
                        .Where(s => s.Quantity > 0 && s.ExpirationDate > today)
                        .ToList();

                    var stock = cart.PharmacyId.HasValue
                        ? activeStocks.FirstOrDefault(s => s.PharmacyId == cart.PharmacyId.Value)
                        : null;

                    return new CartItemDTO
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,

                        UnitPrice = stock != null ? stock.Price : (activeStocks.Any() ? activeStocks.Min(s => s.Price) : 0),

                        DiscountPercentage = stock != null ? stock.DiscountPercentage : null,

                        PictureUrl = ci.Product.PictureUrl,
                        Quantity = ci.Quantity
                    };
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

        [AllowAnonymous]
        [HttpPost("recalculate/{pharmacyId}")]
        public async Task<ActionResult<List<CartItemDTO>>> RecalculatePrice(int pharmacyId, [FromBody] List<AddToCartDTO> items)
        {
            var result = new List<CartItemDTO>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var item in items)
            {
                var product = await _context.Products.Include(p => p.Stocks).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null) continue;

                var activeStocks = product.Stocks.Where(s => s.Quantity > 0 && s.ExpirationDate > today).ToList();

                var stock = pharmacyId > 0 ? activeStocks.FirstOrDefault(s => s.PharmacyId == pharmacyId) : null;

                result.Add(new CartItemDTO
                {
                    ProductId = product.Id,
                    ProductName = product.Name,

                    UnitPrice = stock != null ? stock.Price : (activeStocks.Any() ? activeStocks.Min(s => s.Price) : 0),

                    DiscountPercentage = stock != null ? stock.DiscountPercentage : null,

                    PictureUrl = product.PictureUrl,
                    Quantity = item.Quantity
                });
            }
            return Ok(result);
        }

        [HttpPut("pharmacy/{pharmacyId}")]
        public async Task<ActionResult> SetPharmacy(int pharmacyId)
        {
            var cart = await GetOrCreateCartAsync(GetUserId());
            cart.PharmacyId = pharmacyId == 0 ? null : pharmacyId;
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
