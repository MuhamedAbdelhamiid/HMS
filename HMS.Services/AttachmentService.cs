using HMS.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HMS.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png"];
        private readonly int _maxSize = 5 * 1024 * 1024;
        private readonly ILogger<AttachmentService> _logger;

        public AttachmentService(ILogger<AttachmentService> logger, IConfiguration configuration)
        {
            _logger = logger;
        }

        public bool Delete(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error happened while deleting file: {filePath}");
                return false;
            }
        }

        public async Task<string?> UploadAsync(string mainFolder, string innerFolderName, IFormFile file)
        {
            try
            {
                // check for nullability of folder name and file and file length
                if (string.IsNullOrEmpty(mainFolder) || string.IsNullOrEmpty(innerFolderName) || file is null || file.Length == 0)
                    return null!;

                // check for file size that not exceed the allowed size
                if (file.Length > _maxSize)
                    return null!;

                // get the extension and make sure that is allowed
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!_allowedExtensions.Contains(fileExtension))
                    return null!;

                // check for directory existance
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", mainFolder, innerFolderName);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // build a fileName using guid along with file name
                var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;

                // get the file path combined of folder and fileName
                var filePath = Path.Combine(folderPath, fileName);
                // open stream 

                using var stream = new FileStream(filePath, FileMode.Create);
                // copy the file to the stream 
                await file.CopyToAsync(stream);
                // return the fileName
                return $"{mainFolder}/{innerFolderName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while uploading file with this info " +
                    $"File: {file}, Main Folder: {mainFolder}, Inner Folder: {innerFolderName}.");
                return null!;
            }
        }
    }
}
