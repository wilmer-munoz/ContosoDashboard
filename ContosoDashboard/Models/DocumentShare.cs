using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int SharedWithUserId { get; set; }

    [Required]
    public int SharedByUserId { get; set; }

    [Required]
    public DateTime SharedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string Permissions { get; set; } = "read"; // read, download, etc.

    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document? Document { get; set; }

    [ForeignKey("SharedWithUserId")]
    public virtual User? SharedWithUser { get; set; }

    [ForeignKey("SharedByUserId")]
    public virtual User? SharedByUser { get; set; }
}