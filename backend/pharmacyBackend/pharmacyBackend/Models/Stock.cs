using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}
