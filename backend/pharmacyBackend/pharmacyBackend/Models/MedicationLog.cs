namespace pharmacyBackend.Models
{
    public class MedicationLog
    {
        public int Id { get; set; }

        public int MedicationReminderId { get; set; }
        public MedicationReminder MedicationReminder { get; set; } = null!;

        public DateTime TakenDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;

        public DateTime ActualTakenAt { get; set; }
    }
}
