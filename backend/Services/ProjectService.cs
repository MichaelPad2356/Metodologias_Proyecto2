using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ProjectService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string? userName = null)
    {
        // Validar que el código no exista
        var existingProject = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == dto.Code);

        if (existingProject != null)
        {
            return Result<ProjectDto>.Fail($"Ya existe un proyecto con el código '{dto.Code}'");
        }

        var project = new Project
        {
            Name = dto.Name,
            Code = dto.Code,
            StartDate = dto.StartDate,
            Description = dto.Description,
                Objetivos = dto.Objetivos,
                Alcance = dto.Alcance,
                CronogramaInicial = dto.CronogramaInicial,
                Responsables = dto.Responsables,
                Hitos = dto.Hitos,
            ResponsiblePerson = dto.ResponsiblePerson,
            Tags = dto.Tags,
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Crear las 4 fases predeterminadas de OpenUP
        var phases = new List<ProjectPhase>
        {
            new() { Name = "Inicio", Order = 1, Status = PhaseStatus.NotStarted },
            new() { Name = "Elaboración", Order = 2, Status = PhaseStatus.NotStarted },
            new() { Name = "Construcción", Order = 3, Status = PhaseStatus.NotStarted },
            new() { Name = "Transición", Order = 4, Status = PhaseStatus.NotStarted }
        };

        project.Phases = phases;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Registrar auditoría
        await _auditService.LogActionAsync(
            project.Id,
            "CreateProject",
            "Project",
            project.Id,
            userName,
            $"Proyecto '{project.Name}' creado con código '{project.Code}'"
        );

        return Result<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Phases.OrderBy(ph => ph.Order))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return Result<ProjectDto>.Fail($"No se encontró el proyecto con ID {id}");
        }

        return Result<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> GetProjectByCodeAsync(string code)
    {
        var project = await _context.Projects
            .Include(p => p.Phases.OrderBy(ph => ph.Order))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code);

        if (project == null)
        {
            return Result<ProjectDto>.Fail($"No se encontró el proyecto con código '{code}'");
        }

        return Result<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<Result<List<ProjectListDto>>> GetAllProjectsAsync(bool includeArchived = false)
    {
        var query = _context.Projects
            .Include(p => p.Phases)
            .AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(p => p.Status != ProjectStatus.Archived);
        }

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectListDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                StartDate = p.StartDate,
                ResponsiblePerson = p.ResponsiblePerson,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                PhaseCount = p.Phases.Count
            })
            .ToListAsync();

        return Result<List<ProjectListDto>>.Ok(projects);
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, string? userName = null)
    {
        var project = await _context.Projects
            .Include(p => p.Phases)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return Result<ProjectDto>.Fail($"No se encontró el proyecto con ID {id}");
        }

        var changes = new List<string>();

        if (dto.Name != null && dto.Name != project.Name)
        {
            changes.Add($"Nombre: '{project.Name}' → '{dto.Name}'");
            project.Name = dto.Name;
        }

        if (dto.Description != null && dto.Description != project.Description)
        {
            changes.Add($"Descripción actualizada");
            project.Description = dto.Description;
        }

        if (dto.Objetivos != null && dto.Objetivos != project.Objetivos)
        {
            changes.Add("Objetivos actualizados");
            project.Objetivos = dto.Objetivos;
        }

        if (dto.Alcance != null && dto.Alcance != project.Alcance)
        {
            changes.Add("Alcance actualizado");
            project.Alcance = dto.Alcance;
        }

        if (dto.CronogramaInicial != null && dto.CronogramaInicial != project.CronogramaInicial)
        {
            changes.Add("Cronograma inicial actualizado");
            project.CronogramaInicial = dto.CronogramaInicial;
        }

        if (dto.Responsables != null && dto.Responsables != project.Responsables)
        {
            changes.Add("Responsables actualizados");
            project.Responsables = dto.Responsables;
        }

        if (dto.Hitos != null && dto.Hitos != project.Hitos)
        {
            changes.Add("Hitos actualizados");
            project.Hitos = dto.Hitos;
        }

        if (dto.ResponsiblePerson != null && dto.ResponsiblePerson != project.ResponsiblePerson)
        {
            changes.Add($"Responsable: '{project.ResponsiblePerson}' → '{dto.ResponsiblePerson}'");
            project.ResponsiblePerson = dto.ResponsiblePerson;
        }

        if (dto.Tags != null && dto.Tags != project.Tags)
        {
            project.Tags = dto.Tags;
        }

        if (dto.StartDate.HasValue && dto.StartDate.Value != project.StartDate)
        {
            changes.Add($"Fecha de inicio: {project.StartDate:yyyy-MM-dd} → {dto.StartDate.Value:yyyy-MM-dd}");
            project.StartDate = dto.StartDate.Value;
        }

        await _context.SaveChangesAsync();

        if (changes.Any())
        {
            await _auditService.LogActionAsync(
                project.Id,
                "UpdateProject",
                "Project",
                project.Id,
                userName,
                string.Join(", ", changes)
            );
        }

        return Result<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<Result<bool>> ArchiveProjectAsync(int id, string? userName = null)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return Result<bool>.Fail($"No se encontró el proyecto con ID {id}");
        }

        if (project.Status == ProjectStatus.Archived)
        {
            return Result<bool>.Fail("El proyecto ya está archivado");
        }

        project.Status = ProjectStatus.Archived;
        project.ArchivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            project.Id,
            "ArchiveProject",
            "Project",
            project.Id,
            userName,
            $"Proyecto '{project.Name}' archivado"
        );

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> UnarchiveProjectAsync(int id, string? userName = null)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return Result<bool>.Fail($"No se encontró el proyecto con ID {id}");
        }

        if (project.Status != ProjectStatus.Archived)
        {
            return Result<bool>.Fail("El proyecto no está archivado");
        }

        project.Status = ProjectStatus.Active;
        project.ArchivedAt = null;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            project.Id,
            "UnarchiveProject",
            "Project",
            project.Id,
            userName,
            $"Proyecto '{project.Name}' desarchivado"
        );

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> DeleteProjectAsync(int id, string? userName = null)
    {
        var project = await _context.Projects
            .Include(p => p.Phases)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return Result<bool>.Fail($"No se encontró el proyecto con ID {id}");
        }

        var projectName = project.Name;
        var projectCode = project.Code;

        // Registrar en auditoría antes de eliminar
        await _auditService.LogActionAsync(
            id,
            "DeleteProject",
            "Project",
            id,
            userName,
            $"Proyecto '{projectName}' (código: '{projectCode}') será eliminado permanentemente"
        );

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            StartDate = project.StartDate,
            Description = project.Description,
            ResponsiblePerson = project.ResponsiblePerson,
            Tags = project.Tags,
            Objetivos = project.Objetivos,
            Alcance = project.Alcance,
            CronogramaInicial = project.CronogramaInicial,
            Responsables = project.Responsables,
            Hitos = project.Hitos,
            Status = project.Status.ToString(),
            CreatedAt = project.CreatedAt,
            ArchivedAt = project.ArchivedAt,
            Phases = project.Phases
                .OrderBy(p => p.Order)
                .Select(p => new ProjectPhaseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Order = p.Order,
                    Status = p.Status.ToString()
                })
                .ToList()
        };
    }
}
