using pharmacyBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace pharmacyBackend.DTO
{
    public class ReminderCreateDTO
    {
        public int? ProductId { get; set; }
        [Required] public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TimesOfDay { get; set; } = string.Empty;

        public MedicationFrequency Frequency { get; set; }
        public int? IntervalDays { get; set; }
        public string? DaysOfWeek { get; set; }

        public bool RemindToBuy { get; set; }
        public int RemindToBuyMethod { get; set; } = 1;

        public int? PackQuantity { get; set; }
        public int? PillsPerPack { get; set; }
        public double? PillsPerDay { get; set; }
    }

    public class ReminderDTO : ReminderCreateDTO
    {
        public int Id { get; set; }
        public List<MedicationLogDTO> Logs { get; set; } = new();
    }

    public class MedicationLogDTO
    {
        public int Id { get; set; }
        public DateTime TakenDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public DateTime ActualTakenAt { get; set; }
    }
}
