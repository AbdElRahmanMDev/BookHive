
using BookHive.Web.consts;
using BookHive.Web.Core.Models;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private List<string> _allowedExtensions = new() { ".jpg", ".png", ".jpeg" };
        private int _maxAllowedSize = 2097152;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _environment = webHostEnvironment;
        }

        public void Delete(string imagePath, string? imageThumbnailPath = null)
        {
            var oldImage = $"{_environment.WebRootPath}{imagePath}";
            if (File.Exists(oldImage))
                File.Delete(oldImage);

            if (!string.IsNullOrEmpty(imageThumbnailPath))
            {
                var oldImageThumb = $"{_environment.WebRootPath}{imageThumbnailPath}";
                if (File.Exists(oldImageThumb))
                    File.Delete(oldImageThumb);

            }

          
        }

        public async Task<(bool IsUploaded, string? errorMessage)> UploadAsync(
            IFormFile image, 
            string imageName, 
            string folderPath, 
            bool hasThumbnail) 
        {
            var extension = Path.GetExtension(image.FileName);

            if (!_allowedExtensions.Contains(extension))
                return (false,Validationscs.AllowedExtension);

            if (image.Length > _maxAllowedSize)
                return (IsUploaded: false, errorMessage: Validationscs.MaxSize);

            var path = Path.Combine($"{_environment.WebRootPath}{folderPath}", imageName);

            using var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();

            if (hasThumbnail)
            {
                var thumbPath = Path.Combine($"{_environment.WebRootPath}{folderPath}/thumb", imageName);

                using var loadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedImage.Width / 200;
                var height = loadedImage.Height / ratio;
                loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
                loadedImage.Save(thumbPath);
            }

            return (IsUploaded: true, errorMessage: null);
        }
    }
}
