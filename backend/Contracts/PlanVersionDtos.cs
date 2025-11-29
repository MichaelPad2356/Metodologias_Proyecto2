using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public record SavePlanVersionDto
{
    [StringLength(1000)]
    public string? Observaciones { get; set; }
}

public record PlanVersionDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int Version { get; set; }
    public string? Objetivos { get; set; }
    public string? Alcance { get; set; }
    public string? CronogramaInicial { get; set; }
    public string? Responsables { get; set; }
    public string? Hitos { get; set; }
    public string? Observaciones { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
