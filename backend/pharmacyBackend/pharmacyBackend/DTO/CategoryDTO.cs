using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<String> Tags { get; set; } = new();
    }

    public class CategoryFullDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> Tags { get; set; } = new();
    }

    public class CategoryCreateDTO
    {
        [Required(ErrorMessage = "Название категории обязательно.")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
        public string? Description { get; set; }
        public List<int> TagIds { get; set; } = new();
    }

    public class TagCreateDTO
    {
        [Required(ErrorMessage = "Название тега обязательно.")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
