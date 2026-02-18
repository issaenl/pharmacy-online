namespace pharmacyBackend.DTO
{
    public class ProductFilterParams
    {
        public string? CategoryIds { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool? IsPrescription { get; set; }
        public string? Country { get; set; }
        public string? Manufacturer { get; set; }
        public string? District { get; set; }
    }
}