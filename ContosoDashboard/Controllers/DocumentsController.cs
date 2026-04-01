using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContosoDashboard.Services;
using ContosoDashboard.Models;
using System.Security.Claims;

namespace ContosoDashboard.Controllers;

[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorage;

    public DocumentsController(IDocumentService documentService, IFileStorageService fileStorage)
    {
        _documentService = documentService;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            // Get document with access check
            var document = await _documentService.GetDocumentAsync(id, userId);
            if (document == null)
            {
                return NotFound();
            }

            // Get file stream
            var fileStream = await _fileStorage.GetFileAsync(document.FilePath);
            if (fileStream == null)
            {
                return NotFound("File not found on disk");
            }

            // Log download action (if audit service is available)
            // await _auditService.LogDocumentActionAsync(id, userId, "Download", $"Downloaded file: {document.Title}");

            return File(fileStream, document.FileType, document.Title + GetFileExtension(document.FileType));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private string GetFileExtension(string contentType)
    {
        return contentType switch
        {
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "application/vnd.ms-excel" => ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            "application/vnd.ms-powerpoint" => ".ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
            "text/plain" => ".txt",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            _ => ""
        };
    }
}