using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data;
namespace pharmacyBackend.DTO
{
    //public class CreateOrderDTO
    //{
    //    [Required(ErrorMessage = "Выберите аптеку для самовывоза.")]
    //    public int PharmacyId { get; set; }
    //}

    public class UpdateOrderStatusDTO
    {
        [Required]
        public OrderStatus Status { get; set; }
    }

    public class QuickCheckoutDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int PharmacyId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReadyDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusName => Status.ToString();

    }

    public class OrderWithItemsDTO : OrderDTO
    {
        public string PharmacyAddress { get; set; } = string.Empty;
        public bool HasReview { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
    }

    public class OrderFullDTO : OrderWithItemsDTO
    {
        public string UserFirstName {  get; set; } = string.Empty;
        public string UserPhone {  get; set; } = string.Empty;
    }

    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Price * Quantity;

    }
}
