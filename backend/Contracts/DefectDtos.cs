namespace backend.Contracts;

public class DefectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int? ArtifactId { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDefectDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low";
    public string Status { get; set; } = "New";
    public int ProjectId { get; set; }
    public int? ArtifactId { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateDefectDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
}
