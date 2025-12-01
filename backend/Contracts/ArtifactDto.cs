using backend.Models;

namespace backend.Contracts;

public class ArtifactVersionDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DownloadUrl { get; set; }
}

public class ArtifactDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArtifactType Type { get; set; }
    public ArtifactStatus Status { get; set; }
    public bool IsMandatory { get; set; }
    public int? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public int? CurrentStepId { get; set; }
    public string? CurrentStepName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}