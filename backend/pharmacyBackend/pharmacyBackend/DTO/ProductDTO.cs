using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class ProductShortDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public string? PictureUrl { get; set; } 
        public decimal MinPrice { get; set; }
    }

    public class ProductMediumDTO : ProductShortDTO
    {
        public string Manufacturer { get; set; } = string.Empty;
        public string Country {  get; set; } = string.Empty;
        public bool IsPrescription { get; set; }

        public string CategoryName {  get; set; } = string.Empty;
        public decimal MaxPrice { get; set; }
    }

    public class ProductLongDTO : ProductMediumDTO
    {
        public DateOnly ExpirationDate { get; set; }
        public string? PdfUrl { get; set; }
        public List<PharmacyStockDTO> AvailableIn { get; set; } = new();
    }

    public class ProductFullDTO : ProductLongDTO
    {
        public bool IsActive { get; set; }
    }


    public class ProductCreateDTO
    {
        [Required(ErrorMessage = "Название товара обязательно.")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Производитель обязателен.")]
        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Страна обязательна.")]
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        public bool IsPrescription { get; set; }

        [Required(ErrorMessage = "Укажите форму выпуска (таблетки, сироп и т.д.).")]
        [MaxLength(50)]
        public string DosageForm { get; set; } = string.Empty;

        public DateOnly? ExpirationDate { get; set; }

        public string? PdfUrl { get; set; }
        public string? PictureUrl { get; set; }

        [Required(ErrorMessage = "Товар должен принадлежать категории.")]
        public int CategoryId { get; set; }
    }

    public class ProductUpdateDTO : ProductCreateDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
