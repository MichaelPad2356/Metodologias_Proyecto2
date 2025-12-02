using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public record CreateArtifactDto(
    [Required] ArtifactType Type,
    [Required] int ProjectPhaseId,
    bool IsMandatory = false,
    ArtifactStatus Status = ArtifactStatus.Pending,
    // HU-009: Campos de transición
    string? BuildIdentifier = null,
    string? BuildDownloadUrl = null,
    string? ClosureChecklistJson = null
);

public record CreateArtifactVersionDto(
    [Required] int ArtifactId,
    [Required] string Author,
    string? Observations = null,
    string? Content = null
    // File-related properties are handled via form data
);

// HU-009: DTO para actualizar información de build
public record UpdateBuildInfoDto(
    string? BuildIdentifier,
    string? BuildDownloadUrl
);

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? RepositoryUrl { get; set; }
    public string? AssignedTo { get; set; }

    public IFormFile? File { get; set; }
}