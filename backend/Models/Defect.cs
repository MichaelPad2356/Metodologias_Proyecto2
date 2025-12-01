<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;

>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
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
<<<<<<< HEAD
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
=======

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
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
}
