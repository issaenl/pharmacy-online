using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(2), MaxLength(200)]
        public string Name {  get; set; } = string.Empty;

        [Required]
        public string Manufacturer { get; set; } = string.Empty;
        
        [Required]
        public string Country {  get; set; } = string.Empty;

        [Required]
        public bool IsPrescription { get; set; }
       
        [Required]
        public string DosageForm { get; set; } = string.Empty;
       
        public DateOnly ExpirationDate { get; set; } 
        
        public string? PdfUrl { get; set; }
       
        public string? PictureUrl { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
        
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<Stock> Stocks { get; set; }
    }
}
