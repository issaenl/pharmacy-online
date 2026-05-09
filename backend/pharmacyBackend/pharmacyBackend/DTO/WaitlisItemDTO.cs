namespace pharmacyBackend.DTO
{
    public class WaitlistItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string District { get; set; }
        public DateTime AddedAt { get; set; }
        public string PictureUrl { get; set; }
    }

    public class AddWaitlistDto
    {
        public int ProductId { get; set; }
        public string District { get; set; }
    }
}
