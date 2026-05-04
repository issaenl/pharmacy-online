using CloudinaryDotNet.Actions;
namespace pharmacyBackend.Services
{
    public interface ICloudService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string publicId, ResourceType resourceType = ResourceType.Image);
    }
}
