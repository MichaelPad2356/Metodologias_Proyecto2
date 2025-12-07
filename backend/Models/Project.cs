using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? ResponsiblePerson { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }

    [StringLength(2000)]
    public string? Objetivos { get; set; }

    [StringLength(2000)]
    public string? Alcance { get; set; }

    [StringLength(2000)]
    public string? CronogramaInicial { get; set; }

    [StringLength(1000)]
    public string? Responsables { get; set; }

    [StringLength(2000)]
    public string? Hitos { get; set; }

    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Created;

    /// <summary>
    /// Email del usuario que creó el proyecto
    /// </summary>
    [StringLength(200)]
    public string? CreatedByEmail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ArchivedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<ProjectPhase> Phases { get; set; } = new List<ProjectPhase>();
}

public enum ProjectStatus
{
    Created,
    Active,
    Archived,
    Closed
}
