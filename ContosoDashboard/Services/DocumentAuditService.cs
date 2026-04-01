using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentAuditService : IDocumentAuditService
{
    private readonly ApplicationDbContext _context;

    public DocumentAuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogDocumentActionAsync(int documentId, int userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new DocumentAuditLog
        {
            DocumentId = documentId,
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.DocumentAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentAuditLog>> GetDocumentAuditLogsAsync(int documentId)
    {
        return await _context.DocumentAuditLogs
            .Where(a => a.DocumentId == documentId)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentAuditLog>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.DocumentAuditLogs
            .Where(a => a.UserId == userId)
            .Include(a => a.Document)
            .Include(a => a.User)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}