using Microsoft.AspNetCore.Http;

namespace HMS.Services.Abstraction
{
    public interface IAttachmentService
    {
        Task<string?> UploadAsync(string mainFolder, string innerFolderName, IFormFile file);

        bool Delete(string filePath);
    }
}
