using pharmacyBackend.Enums;

namespace pharmacyBackend.DTO
{
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
