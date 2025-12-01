using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public enum ArtifactType
{
    Document,
    Diagram,
    Code,
    TestCase,
    Other
}

public enum ArtifactStatus
{
    Pending,
<<<<<<< HEAD
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
=======
    InProgress,
    UnderReview,
    Approved,
    Delivered
>>>>>>> origin/feature/-entregable
}

public class Artifact
{
    public int Id { get; set; }
    public int ProjectPhaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArtifactType Type { get; set; }
    public ArtifactStatus Status { get; set; }
    public bool IsMandatory { get; set; }
<<<<<<< HEAD

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
=======
    
    // Workflow fields
    public int? WorkflowId { get; set; }
    public int? CurrentStepId { get; set; }
    
    // Author and content
    public string? Author { get; set; }
    public string? Content { get; set; }
    public string? FilePath { get; set; }
    
    // Timestamps - CAMBIADOS A NO NULLABLE
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Workflow? Workflow { get; set; }
    public WorkflowStep? CurrentStep { get; set; }
>>>>>>> origin/feature/-entregable
}