using pharmacyBackend.Enums;
using pharmacyBackend.Models;

namespace pharmacyBackend.Services
{
    public interface IEmailGenerator
    {
        (string Subject, string HtmlBody) GenerateStatusEmail(User user, Order order, OrderStatus status, 
            Cart? cart = null, string? itemsHtml = null, decimal? totalPrice = null);
    }
}
