using backend.Contracts;

namespace backend.Services;

public interface IIterationService
{
    Task<Result<List<IterationDto>>> GetProjectIterationsAsync(int projectId);
    Task<Result<IterationDto>> GetIterationByIdAsync(int id);
    Task<Result<IterationDto>> CreateIterationAsync(int projectId, CreateIterationDto dto);
    Task<Result<IterationDto>> UpdateIterationAsync(int id, UpdateIterationDto dto);
    Task<Result<bool>> DeleteIterationAsync(int id);
    
    Task<Result<IterationTaskDto>> CreateTaskAsync(int iterationId, CreateIterationTaskDto dto);
    Task<Result<IterationTaskDto>> UpdateTaskAsync(int taskId, UpdateIterationTaskDto dto);
    Task<Result<bool>> DeleteTaskAsync(int taskId);
    
    Task<Result<ProjectProgressDto>> GetProjectProgressAsync(int projectId);
}
