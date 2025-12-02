using backend.Models;

namespace backend.Contracts;

public record ArtifactDto(
    int Id,
    ArtifactType Type,
    int ProjectPhaseId,
    string? PhaseName,
    bool IsMandatory,
    ArtifactStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int VersionCount,
    ArtifactVersionDto? LatestVersion,
    // HU-009: Campos de transición
    string? BuildIdentifier,
    string? BuildDownloadUrl,
    string? ClosureChecklistJson
);

public record ArtifactVersionDto(
    int Id,
    int ArtifactId,
    int VersionNumber,
    string Author,
    string? Observations,
    string? Content,
    string? FilePath,
    string? OriginalFileName,
    string? ContentType,
    long? FileSize,
    DateTime CreatedAt
);

// HU-010: DTO para comparación de versiones
public record VersionComparisonDto(
    ArtifactVersionDto Version1,
    ArtifactVersionDto Version2,
    List<string> Changes
);

// HU-010: DTO para exportación de historial
public record VersionHistoryExportDto(
    int ArtifactId,
    string ArtifactType,
    string PhaseName,
    DateTime ExportedAt,
    List<ArtifactVersionDto> Versions
);

// HU-009: DTOs para artefactos de transición
public record ArtifactTypeInfo(ArtifactType Type, string TypeName);

public record TransitionArtifactsResponse(
    int PhaseId,
    List<ArtifactDto> Artifacts,
    List<ArtifactTypeInfo> MandatoryTypes,
    List<ArtifactTypeInfo> MissingMandatory,
    bool CanClose
);

public record ChecklistValidation(bool IsValid, List<string> PendingItems);

public record ClosureValidationResponse(
    bool CanClose,
    List<string> MissingArtifacts,
    List<ArtifactTypeInfo> PendingApproval,
    ChecklistValidation ChecklistValidation
);

public record ChecklistItem
{
    public string Label { get; set; } = "";
    public bool Completed { get; set; }
}