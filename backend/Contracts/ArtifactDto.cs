using backend.Models;

namespace backend.Contracts;

public class ArtifactVersionDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? OriginalFileName { get; set; }
    public string? RepositoryUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DownloadUrl { get; set; }
}

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
    public int Id { get; set; }
    public ArtifactType Type { get; set; }
    public string TypeName => Type.ToString();
    public int ProjectPhaseId { get; set; }
    public bool IsMandatory { get; set; }
    public ArtifactStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ArtifactVersionDto> Versions { get; set; } = new();
}