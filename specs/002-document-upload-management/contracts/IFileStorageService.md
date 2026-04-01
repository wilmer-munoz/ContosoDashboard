# IFileStorageService Contract

## Interface Definition

```csharp
public interface IFileStorageService
{
    Task<string> StoreFileAsync(IFormFile file, string fileName);
    Task<Stream> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    bool FileExists(string filePath);
}
```

## Contract Details

- **StoreFileAsync**: Saves the uploaded file and returns the secure file path. Generates unique filename.
- **GetFileAsync**: Returns a stream for downloading the file.
- **DeleteFileAsync**: Removes the file from storage.
- **GetFileSizeAsync**: Returns file size in bytes.
- **FileExists**: Checks if file exists without opening.

## Implementation Notes

- Paths are relative to storage root.
- Files stored outside wwwroot.
- Unique GUID-based names for security.