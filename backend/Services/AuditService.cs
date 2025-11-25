using backend.Data;
using backend.Models;

namespace backend.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(
        int projectId,
        string action,
        string entityType,
        int entityId,
        string? userName,
        string? details)
    {
        var auditLog = new AuditLog
        {
            ProjectId = projectId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserName = userName ?? "Sistema",
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
