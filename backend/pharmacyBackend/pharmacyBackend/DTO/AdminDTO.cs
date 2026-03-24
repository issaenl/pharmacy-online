namespace pharmacyBackend.DTO
{
    public class TopProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }

    public class ExpiringProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateOnly ExpirationDate { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
    }

    public class LowStockProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string PharmacyName { get; set; } = string.Empty; 
    }

    public class GeneralAdminDTO
    {
        public decimal TotalRevenue { get; set; }
        public int OrdersToday { get; set; }
        public int ActivePharmacies { get; set; }
        public int TotalProducts { get; set; }
        public int GlobalLowStockCount { get; set; }
        public List<TopProductDTO> TopProducts { get; set; } = new();
        public List<ExpiringProductDTO> ExpiringProducts { get; set; } = new();
        public List<LowStockProductDTO> LowStockProducts { get; set; } = new();
    }

    public class PharmacyAdminDTO
    {
        public decimal RevenueToday { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedTodayOrders { get; set; }
        public int LowStockCount { get; set; }
        public List<TopProductDTO> TopProducts { get; set; } = new();
        public List<ExpiringProductDTO> ExpiringProducts { get; set; } = new();
        public List<LowStockProductDTO> LowStockProducts { get; set; } = new();
    }
}
