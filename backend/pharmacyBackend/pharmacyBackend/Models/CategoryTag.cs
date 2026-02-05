using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.Models
{
    public class CategoryTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required]
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
