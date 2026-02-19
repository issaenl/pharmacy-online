using pharmacyBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class PharmacyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public double? Rating { get; set; }
    }

    public class PharmacyFullDTO : PharmacyDTO
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class PharmacyCreateDTO
    {
        [Required(ErrorMessage = "Название аптеки обязательно.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 100 символов.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адрес обязателен.")]
        [MaxLength(200, ErrorMessage = "Адрес слишком длинный.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите район.")]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен.")]
        public string Phone { get; set; } = string.Empty;

        [Range(0.0, 5.0, ErrorMessage = "Рейтинг должен быть от 0 до 5")]
        public double? Rating { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Широта должна быть от -90 до 90.")]
        public double? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Долгота должна быть от -180 до 180.")]
        public double? Longitude { get; set; }
    }

    public class PharmacyStockDTO
    {
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public string? PharmacyPhone { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class PharmacyStockFullDTO : PharmacyStockDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }

    public class StockCreateDTO
    {
        [Required(ErrorMessage = "Укажите ID аптеки.")]
        public int PharmacyId { get; set; }

        [Required(ErrorMessage = "Укажите ID товара.")]
        public int ProductId { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Количество должно быть от 0 до 10000.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Цена должна быть больше нуля.")]
        public decimal Price { get; set; }
    }
}
