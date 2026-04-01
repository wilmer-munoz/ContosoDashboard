using Microsoft.AspNetCore.Http;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document> UploadDocumentAsync(IFormFile file, DocumentMetadata metadata, int userId);
    Task<IEnumerable<Document>> GetUserDocumentsAsync(int userId);
    Task<IEnumerable<Document>> GetProjectDocumentsAsync(int projectId, int userId);
    Task<Document> GetDocumentAsync(int documentId, int userId);
    Task UpdateDocumentMetadataAsync(int documentId, DocumentMetadata metadata, int userId);
    Task DeleteDocumentAsync(int documentId, int userId);
    Task ShareDocumentAsync(int documentId, int shareWithUserId, string permissions, int sharedByUserId);
    Task<(IEnumerable<Document> Documents, int TotalCount)> SearchDocumentsPaginatedAsync(string searchTerm, int userId, string? category = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null, string sortBy = "uploadDate", bool sortDescending = true, int page = 1, int pageSize = 20);
    Task<IEnumerable<Document>> GetRecentDocumentsAsync(int userId, int limit = 5);
}

public class DocumentMetadata
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? Tags { get; set; }
}