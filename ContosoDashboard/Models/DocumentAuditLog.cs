using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentAuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [ForeignKey("DocumentId")]
    public Document? Document { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // "Upload", "Download", "Share", "Delete", "Update"

    [MaxLength(500)]
    public string? Details { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}