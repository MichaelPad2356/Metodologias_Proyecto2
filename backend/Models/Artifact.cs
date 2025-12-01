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
    InProgress,
    UnderReview,
    Approved,
    Delivered
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
}