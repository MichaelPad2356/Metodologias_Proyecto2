using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// HU-018: Configuración global del sistema - Roles personalizados
/// </summary>
public class SystemRole
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? PermissionsJson { get; set; } // JSON con permisos específicos

    public bool IsSystem { get; set; } = false; // Si es true, no se puede eliminar

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// HU-018: Tipos de artefactos personalizados
/// </summary>
public class CustomArtifactType
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? DefaultPhase { get; set; } // Fase por defecto donde se crea

    public bool IsMandatoryByDefault { get; set; } = false;

    public string? CustomFieldsJson { get; set; } // Campos personalizados en JSON

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// HU-018: Etapas/Fases personalizadas
/// </summary>
public class CustomPhaseDefinition
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int Order { get; set; }

    public string? MandatoryArtifactTypesJson { get; set; } // JSON con tipos obligatorios

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// HU-018: Historial de cambios de configuración
/// </summary>
public class ConfigurationHistory
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty; // Role, ArtifactType, Phase, Workflow

    public int EntityId { get; set; }

    [Required]
    [StringLength(20)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete

    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }

    [StringLength(100)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
