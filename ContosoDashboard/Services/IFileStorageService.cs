using Microsoft.AspNetCore.Http;

namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> StoreFileAsync(IFormFile file, string fileName);
    Task<Stream> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    bool FileExists(string filePath);
}