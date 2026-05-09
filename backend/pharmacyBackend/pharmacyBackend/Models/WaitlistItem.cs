using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.Models
{
    public class WaitlistItem
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string District { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
