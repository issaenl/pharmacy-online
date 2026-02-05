using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Ready = 1,
        Completed = 2,
        Cancelled = 3,
        Expired = 4,
        Failed = 5
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Required]
        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }
       
        [Required]
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
