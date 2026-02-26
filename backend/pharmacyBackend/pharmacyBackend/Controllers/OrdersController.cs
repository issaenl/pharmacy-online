using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.Models;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout()
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Stocks)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest("Корзина пуста");
            }

            if(!cart.PharmacyId.HasValue)
            {
                return BadRequest("Не выбрана аптека");
            }

            var order = new Order
            {
                UserId = userId,
                PharmacyId = cart.PharmacyId.Value,
                OrderDate = DateTime.UtcNow,
                Status = Enums.OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            decimal totalOrderPrice = 0;

            foreach (var item in cart.CartItems)
            {
                var stock = item.Product.Stocks.FirstOrDefault(s => s.PharmacyId == cart.PharmacyId.Value);

                if (stock == null || stock.Quantity < item.Quantity)
                {
                    return BadRequest($"Недостаточно товаар {item.Product.Name} в выбранной аптеке");
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = stock.Price
                });

                totalOrderPrice += stock.Price * item.Quantity;

                stock.Quantity -= item.Quantity;
            }

            order.TotalPrice = totalOrderPrice;

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.PharmacyId = null;

            await _context.SaveChangesAsync();

            return Ok(new { orderId = order.Id, message = "Бронь успешно оформлена" });
        }
    }
}
