using backend.Contracts;

namespace backend.Services;

public interface IProjectService
{
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string? userName = null);
    Task<Result<ProjectDto>> GetProjectByIdAsync(int id);
    Task<Result<ProjectDto>> GetProjectByCodeAsync(string code);
    Task<Result<List<ProjectListDto>>> GetAllProjectsAsync(bool includeArchived = false);
    Task<Result<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, string? userName = null);
    Task<Result<bool>> ArchiveProjectAsync(int id, string? userName = null);
    Task<Result<bool>> UnarchiveProjectAsync(int id, string? userName = null);
    Task<Result<bool>> DeleteProjectAsync(int id, string? userName = null);
    Task<Result<bool>> UpdatePhaseStatusAsync(int phaseId, string newStatus);
}

public record Result<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}
