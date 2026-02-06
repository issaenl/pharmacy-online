using System.ComponentModel.DataAnnotations;
using pharmacyBackend.Enums;
namespace pharmacyBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role {  get; set; } = UserRole.User;
    }
}
