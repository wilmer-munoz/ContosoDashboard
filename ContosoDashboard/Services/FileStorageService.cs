using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ContosoDashboard.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> StoreFileAsync(IFormFile file, string originalFileName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty", nameof(file));

        // Generate unique filename
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");

        // Ensure uploads directory exists
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for storage
        return Path.Combine("uploads", fileName);
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", fullPath);

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<long> GetFileSizeAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(0L);

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult(fileInfo.Length);
    }

    public bool FileExists(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        return File.Exists(fullPath);
    }

    private string GetFullPath(string filePath)
    {
        return Path.Combine(_environment.ContentRootPath, filePath);
    }
}