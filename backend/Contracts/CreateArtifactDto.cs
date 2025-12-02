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

// HU-009: DTO para actualizar información de build
public record UpdateBuildInfoDto(
    string? BuildIdentifier,
    string? BuildDownloadUrl
);