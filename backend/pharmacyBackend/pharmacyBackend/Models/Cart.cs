using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User {  get; set; } = null!;

        public int? PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
