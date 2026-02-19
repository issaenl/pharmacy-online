using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class AddToCartDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100.")]
        public int Quantity { get; set; }
    }

    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Price => UnitPrice * Quantity;

    }

    public class CartDTO
    {
        public List<CartItemDTO> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Price);
        public int? PharmacyId { get; set; }
    }
}
