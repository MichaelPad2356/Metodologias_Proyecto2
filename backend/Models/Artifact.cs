using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public enum ArtifactStatus
{
    Pending,
    InReview,
    Approved
}

public enum ArtifactType
{
    // Fase Inicio
    VisionDocument,
    StakeholderList,
    InitialRiskList,
    InitialProjectPlan,
    HighLevelUseCaseModel,
    
    // Fase Elaboración
    SoftwareArchitectureDocument,
    DetailedUseCaseModel,
    RefinedRiskList,
    IterationPlan,
    
    // Fase Construcción
    SourceCode,
    UnitTestReport,
    IntegrationTestReport,
    UserGuide,
    
    // Fase Transición (HU-009)
    UserManual,
    TechnicalManual,
    DeploymentPlan,
    ClosureDocument,
    FinalBuild,
    BetaTestReport,
    
    Other
}

public class Artifact
{
    public int Id { get; set; }

    [Required]
    public ArtifactType Type { get; set; }

    [Required]
    public int ProjectPhaseId { get; set; }
    public ProjectPhase? ProjectPhase { get; set; }

    public bool IsMandatory { get; set; }

    [Required]
    public ArtifactStatus Status { get; set; } = ArtifactStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // HU-009: Campos para Build Final
    public string? BuildIdentifier { get; set; }
    public string? BuildDownloadUrl { get; set; }
    
    // HU-009: Campo para Documento de Cierre
    public string? ClosureChecklistJson { get; set; }

    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
}
