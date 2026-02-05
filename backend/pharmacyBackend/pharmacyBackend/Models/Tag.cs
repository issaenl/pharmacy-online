using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
       
        [Required, MinLength(2)]
        public string Name { get; set; } = string.Empty;

        public ICollection<CategoryTag> CategotyTags { get; set; }
    }
}
