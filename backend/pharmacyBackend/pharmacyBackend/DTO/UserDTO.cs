using pharmacyBackend.Enums;

namespace pharmacyBackend.DTO
{
    public class UserRegisterDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Password {  get; set; } = string.Empty;
    }

    public class UserLoginDTO
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserDataDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public string RoleName => Role.ToString();

    }
}
