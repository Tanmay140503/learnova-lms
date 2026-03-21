namespace Learnova.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string fileName, string folder);
    }
}