using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения.")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа.")]
        [MaxLength(100, ErrorMessage = "Имя слишком длинное.")]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required(ErrorMessage = "Телефон обязателен.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Пароль должен содержать минимум 8 символов, одну заглавную, одну строчную букву и одну цифру.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Телефон обязателен.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен.")]
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

    public class UserUpdateDTO
    {
        [Required(ErrorMessage = "Имя обязательно.")]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required(ErrorMessage = "Телефон обязателен.")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        public string? Email { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Введите старый пароль.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Новый пароль должен содержать минимум 8 символов, одну заглавную, одну строчную букву и одну цифру.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
