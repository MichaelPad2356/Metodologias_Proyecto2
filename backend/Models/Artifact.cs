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
    VisionDocument,
    StakeholderList,
    InitialRiskList,
    InitialProjectPlan,
    HighLevelUseCaseModel,
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

    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
}