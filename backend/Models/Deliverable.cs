using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Deliverable
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int ProjectPhaseId { get; set; }

    [ForeignKey("ProjectPhaseId")]
    public ProjectPhase? ProjectPhase { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Microincrement> Microincrements { get; set; } = new List<Microincrement>();
}