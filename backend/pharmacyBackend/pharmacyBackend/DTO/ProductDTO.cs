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
}
