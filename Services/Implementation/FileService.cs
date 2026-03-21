using Learnova.Services.Interfaces;

namespace Learnova.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                return true;
            }

            return false;
        }

        public string GetFileUrl(string fileName, string folder)
        {
            return $"/uploads/{folder}/{fileName}";
        }
    }
}