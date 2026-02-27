using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.Data;
using pharmacyBackend.Models;
using pharmacyBackend.Services;
using System.Text;
using pharmacyBackend.Enums;
using pharmacyBackend.DTO;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _email;
        private readonly IEmailGenerator _generator;

        public OrdersController(AppDbContext context, IEmailService emailService, IEmailGenerator emailGenerator)
        {
            _context = context;
            _email = emailService;
            _generator = emailGenerator;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Stocks)
                .Include(c => c.Pharmacy)
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
            var emailItemsList = new StringBuilder();

            foreach (var item in cart.CartItems)
            {
                var stock = item.Product.Stocks.FirstOrDefault(s => s.PharmacyId == cart.PharmacyId.Value);

                if (stock == null || stock.Quantity < item.Quantity)
                {
                    return BadRequest($"Недостаточно товара {item.Product.Name} в выбранной аптеке");
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = stock.Price
                });

                totalOrderPrice += stock.Price * item.Quantity;

                stock.Quantity -= item.Quantity;

                emailItemsList.AppendLine($"<li>{item.Product.Name} - {item.Quantity} шт. x {stock.Price:F2} р.</li>");
            }

            order.TotalPrice = totalOrderPrice;
            var pharmacyForEmail = cart.Pharmacy;
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.PharmacyId = null;

            await _context.SaveChangesAsync();

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                cart.Pharmacy = pharmacyForEmail;
                var (subject, htmlBody) = _generator.GenerateStatusEmail(user, order, OrderStatus.Pending,
                    cart, emailItemsList.ToString(), totalOrderPrice);

                if (!string.IsNullOrEmpty(subject))
                {
                    //не ждем ответа
                    _ = _email.SendEmailAsync(user.Email, subject, htmlBody);
                }
            }

            return Ok(new { orderId = order.Id, message = "Бронь успешно оформлена" });
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderWithItemsDTO>>> GetUserOrders()
        {
            var userId = GetUserId();
            var orders = await _context.Orders
                .Include(o => o.Pharmacy)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if(!orders.Any())
            {
                return Ok(new List<OrderWithItemsDTO>());
            }

            var ordersDtos = orders.Select(o => new OrderWithItemsDTO
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                PharmacyId = o.PharmacyId,
                PharmacyName = o.Pharmacy.Name,
                PharmacyAddress = o.Pharmacy.Address,
                Status = o.Status,
                Items = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            }).ToList();

            return Ok(ordersDtos);
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            var userId = GetUserId();
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Stocks)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if(order == null)
            {
                return NotFound("Заказ не найден");
            }

            if(order.Status != OrderStatus.Pending)
            {
                return BadRequest("Можно отменить только ожидающие сборки заказы");
            }

            foreach(var item in order.OrderItems)
            {
                var stock = item.Product.Stocks.FirstOrDefault(s => s.PharmacyId == order.PharmacyId);
                if(stock != null)
                {
                    stock.Quantity = item.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
            {
                var (subject, htmlBody) = _generator.GenerateStatusEmail(order.User, order, OrderStatus.Cancelled);

                if (!string.IsNullOrEmpty(subject))
                {
                    //не ждем ответа
                    _ = _email.SendEmailAsync(order.User.Email, subject, htmlBody);
                }
            }

            return Ok(new { message = "Бронь успешно отменена" });
        }
    }
}
