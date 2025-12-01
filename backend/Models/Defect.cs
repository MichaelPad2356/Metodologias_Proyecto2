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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; } = DefectSeverity.Medium;
    public DefectStatus Status { get; set; } = DefectStatus.New;
    public int ProjectId { get; set; }
    public int? ArtifactId { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Project? Project { get; set; }
    public Artifact? Artifact { get; set; }
}
