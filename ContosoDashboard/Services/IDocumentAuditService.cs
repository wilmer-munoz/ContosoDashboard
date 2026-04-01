using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentAuditService
{
    Task LogDocumentActionAsync(int documentId, int userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<DocumentAuditLog>> GetDocumentAuditLogsAsync(int documentId);
    Task<IEnumerable<DocumentAuditLog>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
}