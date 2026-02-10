using pharmacyBackend.Models;

namespace pharmacyBackend.DTO
{
    public class PharmacyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public double? Rating { get; set; }
    }

    public class PharmacyFullDTO : PharmacyDTO
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class PharmacyStockDTO
    {
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public string? PharmacyPhone { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class PharmacyStockFullDTO : PharmacyDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
