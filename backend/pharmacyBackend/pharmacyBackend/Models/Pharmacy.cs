using System.ComponentModel.DataAnnotations;
namespace pharmacyBackend.Models
{
    public class Pharmacy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        
        [Required] 
        public string District { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public double? Rating { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}
