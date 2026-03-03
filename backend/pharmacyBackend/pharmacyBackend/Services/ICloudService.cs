namespace pharmacyBackend.Services
{
    public interface ICloudService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
