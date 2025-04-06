namespace BookHive.Web.Services
{
    public interface IImageService
    {
        Task<(bool IsUploaded, string? errorMessage)> UploadAsync(IFormFile image,string imageName,string foldrPath,bool hasThumbnail);

        void Delete(string imagePath, string? imageThumbnailPath = null);
    }
}
