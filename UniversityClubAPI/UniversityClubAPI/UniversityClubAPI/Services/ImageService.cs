using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace UniversityClubAPI.Services
{
    public class ImageService
    {
        private readonly Cloudinary _cloudinary;
        public ImageService(IConfiguration config)
        {
            var account = new Account(
                config["CloudinarySettings:CloudName"],
                config["CloudinarySettings:ApiKey"],
                config["CloudinarySettings:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "university-club"
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
            return result.SecureUrl.ToString();
        }
        public async Task<string?> UploadVideoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;
            await using var stream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "university-club/stories"
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException($"Video upload failed: {result.Error.Message}");
            return result.SecureUrl.ToString();
        }
    }
}