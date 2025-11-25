using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ProjectPhase
{
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }

    [Required]
    public PhaseStatus Status { get; set; } = PhaseStatus.NotStarted;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project Project { get; set; } = null!;
}

public enum PhaseStatus
{
    NotStarted,
    InProgress,
    Completed
}
