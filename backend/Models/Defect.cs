using System.ComponentModel.DataAnnotations;

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

    public DefectSeverity Severity { get; set; } = DefectSeverity.Medium;

    public DefectStatus Status { get; set; } = DefectStatus.New;

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? ArtifactId { get; set; }
    public Artifact? Artifact { get; set; }

    [MaxLength(100)]
    public string? ReportedBy { get; set; }

    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
