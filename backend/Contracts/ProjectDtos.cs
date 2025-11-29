using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public record CreateProjectDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime StartDate { get; init; }

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; init; }

    [StringLength(2000, ErrorMessage = "Los objetivos no pueden exceder 2000 caracteres")]
    public string? Objetivos { get; init; }

    [StringLength(2000, ErrorMessage = "El alcance no puede exceder 2000 caracteres")]
    public string? Alcance { get; init; }

    [StringLength(2000, ErrorMessage = "El cronograma inicial no puede exceder 2000 caracteres")]
    public string? CronogramaInicial { get; init; }

    [StringLength(1000, ErrorMessage = "Los responsables no pueden exceder 1000 caracteres")]
    public string? Responsables { get; init; }

    [StringLength(2000, ErrorMessage = "Los hitos no pueden exceder 2000 caracteres")]
    public string? Hitos { get; init; }

    [StringLength(200, ErrorMessage = "El responsable no puede exceder 200 caracteres")]
    public string? ResponsiblePerson { get; init; }

    [StringLength(500, ErrorMessage = "Los tags no pueden exceder 500 caracteres")]
    public string? Tags { get; init; }
}

public record UpdateProjectDto
{
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string? Name { get; init; }

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; init; }

    [StringLength(200, ErrorMessage = "El responsable no puede exceder 200 caracteres")]
    public string? ResponsiblePerson { get; init; }

    [StringLength(500, ErrorMessage = "Los tags no pueden exceder 500 caracteres")]
    public string? Tags { get; init; }

    [StringLength(2000, ErrorMessage = "Los objetivos no pueden exceder 2000 caracteres")]
    public string? Objetivos { get; init; }

    [StringLength(2000, ErrorMessage = "El alcance no puede exceder 2000 caracteres")]
    public string? Alcance { get; init; }

    [StringLength(2000, ErrorMessage = "El cronograma inicial no puede exceder 2000 caracteres")]
    public string? CronogramaInicial { get; init; }

    [StringLength(1000, ErrorMessage = "Los responsables no pueden exceder 1000 caracteres")]
    public string? Responsables { get; init; }

    [StringLength(2000, ErrorMessage = "Los hitos no pueden exceder 2000 caracteres")]
    public string? Hitos { get; init; }

    public DateTime? StartDate { get; init; }
}

public record ProjectDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public string? Description { get; init; }
    public string? ResponsiblePerson { get; init; }
    public string? Tags { get; init; }
    public string? Objetivos { get; init; }
    public string? Alcance { get; init; }
    public string? CronogramaInicial { get; init; }
    public string? Responsables { get; init; }
    public string? Hitos { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ArchivedAt { get; init; }
    public List<ProjectPhaseDto> Phases { get; init; } = new();
}

public record ProjectPhaseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record ProjectListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public string? ResponsiblePerson { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int PhaseCount { get; init; }
}
