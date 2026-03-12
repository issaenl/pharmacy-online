namespace pharmacyBackend.DTO
{
    public class ImportDTO
    {
        public int AddedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string Message => $"Импорт завершен. Добавлено: {AddedCount}. Ошибок: {Errors.Count}.";
    }
}
