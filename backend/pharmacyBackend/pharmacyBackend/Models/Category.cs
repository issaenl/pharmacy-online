using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(2)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; }

        public ICollection<CategoryTag> CategoryTags { get; set; }
    }
}
