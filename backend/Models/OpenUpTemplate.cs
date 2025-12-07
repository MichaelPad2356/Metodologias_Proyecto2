using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// HU-019: Plantilla OpenUP versionada
/// </summary>
public class OpenUpTemplate
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int Version { get; set; } = 1;

    public bool IsDefault { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Configuración serializada en JSON
    public string PhasesJson { get; set; } = "[]"; // Lista de fases con sus configuraciones
    public string RolesJson { get; set; } = "[]"; // Lista de roles
    public string ArtifactTypesJson { get; set; } = "[]"; // Lista de tipos de artefactos
    public string WorkflowsJson { get; set; } = "[]"; // Lista de workflows

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Para versionado
    public int? ParentTemplateId { get; set; }
    public OpenUpTemplate? ParentTemplate { get; set; }
}
