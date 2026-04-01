using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 25 * 1024 * 1024; // 25 MB

    public DocumentService(ApplicationDbContext context, IFileStorageService fileStorage, IDocumentAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _fileStorage = fileStorage;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Document> UploadDocumentAsync(IFormFile file, DocumentMetadata metadata, int userId)
    {
        // Validate file
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 25 MB limit");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("File type not allowed");

        // Store file
        var filePath = await _fileStorage.StoreFileAsync(file, file.FileName);

        // Create document
        var document = new Document
        {
            Title = metadata.Title,
            Description = metadata.Description,
            Category = metadata.Category,
            ProjectId = metadata.ProjectId,
            Tags = metadata.Tags,
            UploadDate = DateTime.UtcNow,
            UploaderId = userId,
            FileSize = file.Length,
            FileType = file.ContentType,
            FilePath = filePath,
            IsDeleted = false
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Log audit event
        await _auditService.LogDocumentActionAsync(
            document.Id,
            userId,
            "Upload",
            $"Uploaded file: {file.FileName} ({FormatFileSize(file.Length)})",
            GetClientIpAddress(),
            GetUserAgent()
        );

        return document;
    }

    public async Task<IEnumerable<Document>> GetUserDocumentsAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploaderId == userId && !d.IsDeleted)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetProjectDocumentsAsync(int projectId, int userId)
    {
        // Check if user has access to project
        var hasAccess = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId) ||
            await _context.Projects.AnyAsync(p => p.ProjectId == projectId && p.ProjectManagerId == userId);

        if (!hasAccess)
            throw new UnauthorizedAccessException("No access to project documents");

        return await _context.Documents
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .Include(d => d.Uploader)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<Document> GetDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        // Check access
        if (document.UploaderId == userId)
            return document;

        // Check project access
        if (document.ProjectId.HasValue)
        {
            var hasProjectAccess = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == document.ProjectId.Value && pm.UserId == userId) ||
                await _context.Projects.AnyAsync(p => p.ProjectId == document.ProjectId.Value && p.ProjectManagerId == userId);

            if (hasProjectAccess)
                return document;
        }

        // Check shares
        var isShared = await _context.DocumentShares
            .AnyAsync(s => s.DocumentId == documentId && s.SharedWithUserId == userId);

        if (isShared)
            return document;

        throw new UnauthorizedAccessException("No access to document");
    }

    public async Task UpdateDocumentMetadataAsync(int documentId, DocumentMetadata metadata, int userId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        if (document.UploaderId != userId)
            throw new UnauthorizedAccessException("Only owner can update metadata");

        document.Title = metadata.Title;
        document.Description = metadata.Description;
        document.Category = metadata.Category;
        document.ProjectId = metadata.ProjectId;
        document.Tags = metadata.Tags;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        // Check permissions
        var canDelete = document.UploaderId == userId;

        if (!canDelete && document.ProjectId.HasValue)
        {
            // Check if user is project manager
            canDelete = await _context.Projects
                .AnyAsync(p => p.ProjectId == document.ProjectId.Value && p.ProjectManagerId == userId);
        }

        if (!canDelete)
            throw new UnauthorizedAccessException("No permission to delete document");

        document.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Log audit event
        await _auditService.LogDocumentActionAsync(
            documentId,
            userId,
            "Delete",
            $"Deleted document: {document.Title}",
            GetClientIpAddress(),
            GetUserAgent()
        );

        // Optionally delete file
        await _fileStorage.DeleteFileAsync(document.FilePath);
    }

    public async Task ShareDocumentAsync(int documentId, int shareWithUserId, string permissions, int sharedByUserId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        if (document.UploaderId != sharedByUserId)
            throw new UnauthorizedAccessException("Only owner can share document");

        var share = new DocumentShare
        {
            DocumentId = documentId,
            SharedWithUserId = shareWithUserId,
            SharedByUserId = sharedByUserId,
            Permissions = permissions,
            SharedDate = DateTime.UtcNow
        };

        _context.DocumentShares.Add(share);
        await _context.SaveChangesAsync();

        // Log audit event
        var sharedUser = await _context.Users.FindAsync(shareWithUserId);
        await _auditService.LogDocumentActionAsync(
            documentId,
            sharedByUserId,
            "Share",
            $"Shared document with {sharedUser?.DisplayName ?? "Unknown User"} (permissions: {permissions})",
            GetClientIpAddress(),
            GetUserAgent()
        );
    }

    public async Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm, int userId, string? category = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null, string sortBy = "uploadDate", bool sortDescending = true, int page = 1, int pageSize = 20)
    {
        var query = _context.Documents
            .Where(d => !d.IsDeleted)
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .AsQueryable();

        // Filter by access
        var accessibleDocuments = await query
            .Where(d => d.UploaderId == userId ||
                       d.ProjectId.HasValue && _context.ProjectMembers.Any(pm => pm.ProjectId == d.ProjectId.Value && pm.UserId == userId) ||
                       _context.DocumentShares.Any(s => s.DocumentId == d.Id && s.SharedWithUserId == userId))
            .ToListAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            accessibleDocuments = accessibleDocuments
                .Where(d => d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (d.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (d.Tags?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (!string.IsNullOrEmpty(category))
        {
            accessibleDocuments = accessibleDocuments
                .Where(d => d.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (projectId.HasValue)
        {
            accessibleDocuments = accessibleDocuments
                .Where(d => d.ProjectId == projectId.Value)
                .ToList();
        }

        if (startDate.HasValue)
        {
            accessibleDocuments = accessibleDocuments
                .Where(d => d.UploadDate >= startDate.Value)
                .ToList();
        }

        if (endDate.HasValue)
        {
            accessibleDocuments = accessibleDocuments
                .Where(d => d.UploadDate <= endDate.Value)
                .ToList();
        }

        // Apply sorting
        accessibleDocuments = sortBy.ToLower() switch
        {
            "title" => sortDescending 
                ? accessibleDocuments.OrderByDescending(d => d.Title).ToList()
                : accessibleDocuments.OrderBy(d => d.Title).ToList(),
            "category" => sortDescending
                ? accessibleDocuments.OrderByDescending(d => d.Category).ToList()
                : accessibleDocuments.OrderBy(d => d.Category).ToList(),
            "size" => sortDescending
                ? accessibleDocuments.OrderByDescending(d => d.FileSize).ToList()
                : accessibleDocuments.OrderBy(d => d.FileSize).ToList(),
            _ => sortDescending
                ? accessibleDocuments.OrderByDescending(d => d.UploadDate).ToList()
                : accessibleDocuments.OrderBy(d => d.UploadDate).ToList()
        };

        // Apply pagination
        return accessibleDocuments
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<(IEnumerable<Document> Documents, int TotalCount)> SearchDocumentsPaginatedAsync(string searchTerm, int userId, string? category = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null, string sortBy = "uploadDate", bool sortDescending = true, int page = 1, int pageSize = 20)
    {
        var query = _context.Documents
            .Where(d => !d.IsDeleted)
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .AsQueryable();

        // Filter by access
        var accessibleQuery = query
            .Where(d => d.UploaderId == userId ||
                       d.ProjectId.HasValue && _context.ProjectMembers.Any(pm => pm.ProjectId == d.ProjectId.Value && pm.UserId == userId) ||
                       _context.DocumentShares.Any(s => s.DocumentId == d.Id && s.SharedWithUserId == userId));

        // Get total count before applying filters
        var totalCount = await accessibleQuery.CountAsync();

        // Apply search filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            accessibleQuery = accessibleQuery
                .Where(d => d.Title.Contains(searchTerm) ||
                           (d.Description != null && d.Description.Contains(searchTerm)) ||
                           (d.Tags != null && d.Tags.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            accessibleQuery = accessibleQuery
                .Where(d => d.Category.Equals(category));
        }

        if (projectId.HasValue)
        {
            accessibleQuery = accessibleQuery
                .Where(d => d.ProjectId == projectId.Value);
        }

        if (startDate.HasValue)
        {
            accessibleQuery = accessibleQuery
                .Where(d => d.UploadDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            accessibleQuery = accessibleQuery
                .Where(d => d.UploadDate <= endDate.Value);
        }

        // Apply sorting
        accessibleQuery = sortBy.ToLower() switch
        {
            "title" => sortDescending 
                ? accessibleQuery.OrderByDescending(d => d.Title)
                : accessibleQuery.OrderBy(d => d.Title),
            "category" => sortDescending
                ? accessibleQuery.OrderByDescending(d => d.Category)
                : accessibleQuery.OrderBy(d => d.Category),
            "size" => sortDescending
                ? accessibleQuery.OrderByDescending(d => d.FileSize)
                : accessibleQuery.OrderBy(d => d.FileSize),
            _ => sortDescending
                ? accessibleQuery.OrderByDescending(d => d.UploadDate)
                : accessibleQuery.OrderBy(d => d.UploadDate)
        };

        // Apply pagination
        var documents = await accessibleQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (documents, totalCount);
    }

    public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(int userId, int limit = 5)
    {
        return await _context.Documents
            .Where(d => !d.IsDeleted)
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Where(d => d.UploaderId == userId ||
                       d.ProjectId.HasValue && _context.ProjectMembers.Any(pm => pm.ProjectId == d.ProjectId.Value && pm.UserId == userId) ||
                       _context.DocumentShares.Any(s => s.DocumentId == d.Id && s.SharedWithUserId == userId))
            .OrderByDescending(d => d.UploadDate)
            .Take(limit)
            .ToListAsync();
    }

    private string GetClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}