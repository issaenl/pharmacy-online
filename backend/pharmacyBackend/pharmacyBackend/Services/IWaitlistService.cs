namespace pharmacyBackend.Services
{
    public interface IWaitlistService
    {
        Task CheckWaitlistOnStockUpdateAsync(int productId, int pharmacyId, int quantity);
    }
}
