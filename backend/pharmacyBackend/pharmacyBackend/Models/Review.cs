using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = null!;

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
