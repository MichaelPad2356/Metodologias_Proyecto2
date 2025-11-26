namespace backend.Contracts;

public class CreateMicroincrementDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectPhaseId { get; set; }
    public int? DeliverableId { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class UpdateMicroincrementDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
}

public class MicroincrementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    public int ProjectPhaseId { get; set; }
    public int? DeliverableId { get; set; }
}