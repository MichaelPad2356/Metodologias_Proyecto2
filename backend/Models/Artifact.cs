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
    UserManual,           // Manual de Usuario
    TechnicalManual,      // Manual Técnico
    DeploymentPlan,       // Plan de Despliegue
    ClosureDocument,      // Documento de Cierre
    FinalBuild,           // Build Final
    BetaTestReport,       // Reporte de Pruebas Beta
    
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

    // Campos específicos para Build Final (HU-009)
    public string? BuildIdentifier { get; set; }  // Identificador único del build (ej: v1.0.0-release)
    public string? BuildDownloadUrl { get; set; } // URL de descarga del build
    
    // Campos para Documento de Cierre (HU-009)
    public string? ClosureChecklistJson { get; set; } // JSON con checklist de criterios cumplidos

    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
}