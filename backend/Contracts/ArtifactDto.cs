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
    public ArtifactType Type { get; set; }
    public string TypeName => Type.ToString();
    public int ProjectPhaseId { get; set; }
    public bool IsMandatory { get; set; }
    public ArtifactStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime CreatedAt { get; set; }
    public List<ArtifactVersionDto> Versions { get; set; } = new();
}