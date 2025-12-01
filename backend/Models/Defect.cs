using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public enum DefectSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DefectStatus
{
    New,
    Assigned,
    Fixed,
    Closed
}

public class Defect
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DefectSeverity Severity { get; set; } = DefectSeverity.Medium;

    [Required]
    public DefectStatus Status { get; set; } = DefectStatus.New;

    [Required]
    public int ProjectId { get; set; }
    
    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    public int? ArtifactId { get; set; }
    
    [ForeignKey("ArtifactId")]
    public Artifact? Artifact { get; set; }

    [MaxLength(100)]
    public string? ReportedBy { get; set; }

    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
