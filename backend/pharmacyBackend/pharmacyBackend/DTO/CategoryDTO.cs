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
}
