using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Defect
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string Severity { get; set; } = "Low";

    [Required]
    public string Status { get; set; } = "New";

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    
    // HU-014: Vinculación a artefactos
    public int? ArtifactId { get; set; }
    public Artifact? Artifact { get; set; }

    public string? ReportedBy { get; set; }
    
    public string? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
