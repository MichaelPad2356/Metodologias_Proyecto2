using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ArtifactVersion
{
    public int Id { get; set; }

    [Required]
    public int ArtifactId { get; set; }
    public Artifact? Artifact { get; set; }

    public int VersionNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    // HU-010: Observaciones - descripción de cambios de esta versión
    [MaxLength(2000)]
    public string? Observations { get; set; }

    // Contenido en línea
    public string? Content { get; set; }

    // Para el archivo subido
    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }  // HU-010: Tamaño del archivo para comparación

    // HU-008: Enlace a repositorio externo
    public string? RepositoryUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}