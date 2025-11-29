using backend.Contracts;

namespace backend.Services;

public interface IPlanVersionService
{
    Task<PlanVersionDto> SavePlanVersionAsync(int projectId, SavePlanVersionDto dto, string userName);
    Task<IEnumerable<PlanVersionDto>> GetPlanVersionsAsync(int projectId);
    Task<PlanVersionDto?> GetPlanVersionAsync(int projectId, int version);
}
