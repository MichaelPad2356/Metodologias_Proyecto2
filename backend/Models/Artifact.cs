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
    // Inception
    VisionDocument,
    StakeholderList,
    InitialRiskList,
    InitialProjectPlan,
    HighLevelUseCaseModel,
    
    // Elaboration (HU-007)
    DetailedUseCaseModel,
    DomainModel,
    SupplRequirements,
    NonFunctionalReqs,
    ArchitectureDoc,
    TechnicalDiagrams,
    IterationPlan,
    UIPrototype,

    // Construction (HU-008)
    DetailedDesignModel,
    SourceCode,
    TestCases,
    TestResults,
    IterationLog,

    // Transition (HU-009)
    UserManual,
    TechnicalManual,
    DeploymentPlan,
    ClosureDoc,
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

    // HU-012: Responsable del estado actual
    public string? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // HU-009: Campos para Build Final
    public string? BuildIdentifier { get; set; }
    public string? BuildDownloadUrl { get; set; }
    
    // HU-009: Campo para Documento de Cierre
    public string? ClosureChecklistJson { get; set; }

    // HU-012: Asociación a Flujo de Trabajo
    public int? WorkflowId { get; set; }
    public Workflow? Workflow { get; set; }

    public int? CurrentStepId { get; set; }
    public WorkflowStep? CurrentStep { get; set; }

    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
}
