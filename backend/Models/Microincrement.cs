using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Microincrement
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(200)]
    public string Author { get; set; } = string.Empty;

    // Foreign Keys
    [Required]
    public int ProjectPhaseId { get; set; }

    [ForeignKey("ProjectPhaseId")]
    public ProjectPhase? ProjectPhase { get; set; }

    public int? DeliverableId { get; set; }

    [ForeignKey("DeliverableId")]
    public Deliverable? Deliverable { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}