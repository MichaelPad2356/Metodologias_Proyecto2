namespace backend.Models;

public class IterationTask
{
    public int Id { get; set; }
    public int IterationId { get; set; }
    public int? ProjectPhaseId { get; set; } // Nueva: vincular tarea a una fase
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PercentageCompleted { get; set; } = 0;
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Blocked
    public string? Blockages { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public Iteration Iteration { get; set; } = null!;
    public ProjectPhase? ProjectPhase { get; set; }
}
