using pharmacyBackend.DTO;

namespace pharmacyBackend.Services
{
    public interface IImportService
    {
        Task<ImportDTO> ImportProductsAsync(IFormFile file);
        Task<ImportDTO> ImportStocksAsync(IFormFile file);
    }
}
