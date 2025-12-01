using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public class CreateArtifactDto
{
    public int ProjectPhaseId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public ArtifactType Type { get; set; }
    
    public bool IsMandatory { get; set; }
    
    public int? WorkflowId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;
<<<<<<< HEAD

    // HU-010: Observaciones para la primera versión
    [MaxLength(2000)]
    public string? Observations { get; set; }

=======
    
>>>>>>> origin/feature/-entregable
    public string? Content { get; set; }
    
    public IFormFile? File { get; set; }
    
    // Campos específicos para Build Final (HU-009)
    public string? BuildIdentifier { get; set; }
    public string? BuildDownloadUrl { get; set; }
    
    // Campos para Documento de Cierre (HU-009)
    public string? ClosureChecklistJson { get; set; }
}

// HU-010: DTO para crear nueva versión con descripción de cambios
public class CreateVersionDto
{
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de cambios es obligatoria al crear una nueva versión")]
    [MaxLength(2000)]
    public string Observations { get; set; } = string.Empty;

    public string? Content { get; set; }

    public IFormFile? File { get; set; }
}

// HU-010: DTO para comparar versiones
public class VersionComparisonDto
{
    public ArtifactVersionDto Version1 { get; set; } = null!;
    public ArtifactVersionDto Version2 { get; set; } = null!;
    public List<string> Differences { get; set; } = new();
}

// HU-010: DTO para historial exportable
public class VersionHistoryExportDto
{
    public string ArtifactType { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string PhaseName { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<VersionHistoryItemDto> Versions { get; set; } = new();
}

public class VersionHistoryItemDto
{
    public int VersionNumber { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
}