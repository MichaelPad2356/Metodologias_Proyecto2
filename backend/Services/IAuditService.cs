namespace backend.Services;

public interface IAuditService
{
    Task LogActionAsync(int projectId, string action, string entityType, int entityId, string? userName, string? details);
}
