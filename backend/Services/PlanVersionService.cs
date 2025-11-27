using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PlanVersionService : IPlanVersionService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public PlanVersionService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<PlanVersionDto> SavePlanVersionAsync(int projectId, SavePlanVersionDto dto, string userName)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException($"Proyecto con ID {projectId} no encontrado");
        }

        // Obtener el número de versión siguiente
        var lastVersion = await _context.ProjectPlanVersions
            .Where(v => v.ProjectId == projectId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync();

        var newVersion = (lastVersion?.Version ?? 0) + 1;

        // Crear la nueva versión con los datos actuales del proyecto
        var planVersion = new ProjectPlanVersion
        {
            ProjectId = projectId,
            Version = newVersion,
            Objetivos = project.Objetivos,
            Alcance = project.Alcance,
            CronogramaInicial = project.CronogramaInicial,
            Responsables = project.Responsables,
            Hitos = project.Hitos,
            Observaciones = dto.Observaciones,
            CreatedBy = userName,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectPlanVersions.Add(planVersion);
        await _context.SaveChangesAsync();

        // Registrar en auditoría
        await _auditService.LogActionAsync(
            projectId,
            "SavePlanVersion",
            "ProjectPlanVersion",
            planVersion.Id,
            userName,
            $"Guardada versión {newVersion} del plan del proyecto. Observaciones: {dto.Observaciones ?? "Sin observaciones"}"
        );

        return new PlanVersionDto
        {
            Id = planVersion.Id,
            ProjectId = planVersion.ProjectId,
            Version = planVersion.Version,
            Objetivos = planVersion.Objetivos,
            Alcance = planVersion.Alcance,
            CronogramaInicial = planVersion.CronogramaInicial,
            Responsables = planVersion.Responsables,
            Hitos = planVersion.Hitos,
            Observaciones = planVersion.Observaciones,
            CreatedBy = planVersion.CreatedBy,
            CreatedAt = planVersion.CreatedAt
        };
    }

    public async Task<IEnumerable<PlanVersionDto>> GetPlanVersionsAsync(int projectId)
    {
        var versions = await _context.ProjectPlanVersions
            .Where(v => v.ProjectId == projectId)
            .OrderByDescending(v => v.Version)
            .ToListAsync();

        return versions.Select(v => new PlanVersionDto
        {
            Id = v.Id,
            ProjectId = v.ProjectId,
            Version = v.Version,
            Objetivos = v.Objetivos,
            Alcance = v.Alcance,
            CronogramaInicial = v.CronogramaInicial,
            Responsables = v.Responsables,
            Hitos = v.Hitos,
            Observaciones = v.Observaciones,
            CreatedBy = v.CreatedBy,
            CreatedAt = v.CreatedAt
        });
    }

    public async Task<PlanVersionDto?> GetPlanVersionAsync(int projectId, int version)
    {
        var planVersion = await _context.ProjectPlanVersions
            .FirstOrDefaultAsync(v => v.ProjectId == projectId && v.Version == version);

        if (planVersion == null) return null;

        return new PlanVersionDto
        {
            Id = planVersion.Id,
            ProjectId = planVersion.ProjectId,
            Version = planVersion.Version,
            Objetivos = planVersion.Objetivos,
            Alcance = planVersion.Alcance,
            CronogramaInicial = planVersion.CronogramaInicial,
            Responsables = planVersion.Responsables,
            Hitos = planVersion.Hitos,
            Observaciones = planVersion.Observaciones,
            CreatedBy = planVersion.CreatedBy,
            CreatedAt = planVersion.CreatedAt
        };
    }
}
