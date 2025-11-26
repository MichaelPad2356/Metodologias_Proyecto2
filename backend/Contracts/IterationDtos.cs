namespace backend.Contracts;

public record IterationDto(
    int Id,
    int ProjectId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    int PercentageCompleted,
    string? Blockages,
    string? Observations,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<IterationTaskDto> Tasks
);

public record IterationTaskDto(
    int Id,
    int IterationId,
    int? ProjectPhaseId,
    string? PhaseName,
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    int PercentageCompleted,
    string? AssignedTo,
    string Status,
    string? Blockages,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateIterationDto(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Observations
);

public record UpdateIterationDto(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    int? PercentageCompleted,
    string? Blockages,
    string? Observations
);

public record CreateIterationTaskDto(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    string? AssignedTo,
    int? ProjectPhaseId
);

public record UpdateIterationTaskDto(
    string? Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    int? PercentageCompleted,
    string? AssignedTo,
    string? Status,
    string? Blockages,
    int? ProjectPhaseId
);

public record ProjectProgressDto(
    int ProjectId,
    int OverallPercentage,
    List<PhaseProgressDto> PhaseProgress,
    List<IterationSummaryDto> RecentIterations
);

public record PhaseProgressDto(
    int PhaseId,
    string PhaseName,
    int PercentageCompleted,
    int TotalIterations,
    int CompletedIterations
);

public record IterationSummaryDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    int PercentageCompleted,
    int TotalTasks,
    int CompletedTasks
);
