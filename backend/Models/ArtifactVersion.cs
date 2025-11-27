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

    // Contenido en línea
    public string? Content { get; set; }

    // Para el archivo subido
    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}