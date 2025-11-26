namespace backend.Models;

public class Iteration
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PercentageCompleted { get; set; } = 0;
    public string? Blockages { get; set; }
    public string? Observations { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public Project Project { get; set; } = null!;
    public ICollection<IterationTask> Tasks { get; set; } = new List<IterationTask>();
}
