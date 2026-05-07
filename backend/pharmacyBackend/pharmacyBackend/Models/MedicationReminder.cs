using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.Models
{
    public class MedicationReminder
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string MedicationName { get; set; } = string.Empty;

        public string Dosage { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TimesOfDay { get; set; } = string.Empty;

        public MedicationFrequency Frequency { get; set; } = MedicationFrequency.Daily;
        public int? IntervalDays { get; set; }

        public string? DaysOfWeek { get; set; }

        public bool RemindToBuy { get; set; }
        public int RemindToBuyMethod { get; set; } = 1;

        public int? PackQuantity { get; set; }
        public int? PillsPerPack { get; set; }
        public double? PillsPerDay { get; set; }

        public List<MedicationLog> Logs { get; set; } = new();
    }
}
