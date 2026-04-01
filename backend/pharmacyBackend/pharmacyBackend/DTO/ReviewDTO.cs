using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class CreateReviewDTO
    {
        [Required(ErrorMessage = "Не указан заказ")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Укажите оценку")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class ReviewAdminDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public ReviewStatus Status { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EditReviewDTO
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
