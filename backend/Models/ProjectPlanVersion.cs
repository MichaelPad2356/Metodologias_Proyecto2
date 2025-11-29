using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ProjectPlanVersion
{
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int Version { get; set; }

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

    [StringLength(1000)]
    public string? Observaciones { get; set; }

    [StringLength(200)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Project Project { get; set; } = null!;
}
