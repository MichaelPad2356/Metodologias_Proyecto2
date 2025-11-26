using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class IterationService : IIterationService
{
    private readonly ApplicationDbContext _context;

    public IterationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<IterationDto>>> GetProjectIterationsAsync(int projectId)
    {
        var iterations = await _context.Iterations
            .Include(i => i.Tasks)
                .ThenInclude(t => t.ProjectPhase)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();

        var dtos = iterations.Select(MapToDto).ToList();
        return Result<List<IterationDto>>.Ok(dtos);
    }

    public async Task<Result<IterationDto>> GetIterationByIdAsync(int id)
    {
        var iteration = await _context.Iterations
            .Include(i => i.Tasks)
                .ThenInclude(t => t.ProjectPhase)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (iteration == null)
        {
            return Result<IterationDto>.Fail("Iteración no encontrada");
        }

        return Result<IterationDto>.Ok(MapToDto(iteration));
    }

    public async Task<Result<IterationDto>> CreateIterationAsync(int projectId, CreateIterationDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return Result<IterationDto>.Fail("Proyecto no encontrado");
        }

        var iteration = new Iteration
        {
            ProjectId = projectId,
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Observations = dto.Observations,
            CreatedAt = DateTime.UtcNow
        };

        _context.Iterations.Add(iteration);
        await _context.SaveChangesAsync();

        return Result<IterationDto>.Ok(MapToDto(iteration));
    }

    public async Task<Result<IterationDto>> UpdateIterationAsync(int id, UpdateIterationDto dto)
    {
        var iteration = await _context.Iterations
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (iteration == null)
        {
            return Result<IterationDto>.Fail("Iteración no encontrada");
        }

        if (dto.Name != null) iteration.Name = dto.Name;
        if (dto.StartDate.HasValue) iteration.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) iteration.EndDate = dto.EndDate.Value;
        if (dto.PercentageCompleted.HasValue) iteration.PercentageCompleted = dto.PercentageCompleted.Value;
        if (dto.Blockages != null) iteration.Blockages = dto.Blockages;
        if (dto.Observations != null) iteration.Observations = dto.Observations;
        
        iteration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<IterationDto>.Ok(MapToDto(iteration));
    }

    public async Task<Result<bool>> DeleteIterationAsync(int id)
    {
        var iteration = await _context.Iterations.FindAsync(id);
        if (iteration == null)
        {
            return Result<bool>.Fail("Iteración no encontrada");
        }

        _context.Iterations.Remove(iteration);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<IterationTaskDto>> CreateTaskAsync(int iterationId, CreateIterationTaskDto dto)
    {
        var iteration = await _context.Iterations.FindAsync(iterationId);
        if (iteration == null)
        {
            return Result<IterationTaskDto>.Fail("Iteración no encontrada");
        }

        var task = new IterationTask
        {
            IterationId = iterationId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AssignedTo = dto.AssignedTo,
            Status = "NotStarted",
            ProjectPhaseId = dto.ProjectPhaseId,
            CreatedAt = DateTime.UtcNow
        };

        _context.IterationTasks.Add(task);
        await _context.SaveChangesAsync();
        
        // Cargar ProjectPhase si existe
        if (task.ProjectPhaseId.HasValue)
        {
            await _context.Entry(task)
                .Reference(t => t.ProjectPhase)
                .LoadAsync();
        }

        // Recalcular porcentaje de la iteración
        await RecalculateIterationProgressAsync(iterationId);

        return Result<IterationTaskDto>.Ok(MapToTaskDto(task));
    }

    public async Task<Result<IterationTaskDto>> UpdateTaskAsync(int taskId, UpdateIterationTaskDto dto)
    {
        var task = await _context.IterationTasks.FindAsync(taskId);
        if (task == null)
        {
            return Result<IterationTaskDto>.Fail("Tarea no encontrada");
        }

        if (dto.Name != null) task.Name = dto.Name;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.StartDate.HasValue) task.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) task.EndDate = dto.EndDate;
        if (dto.PercentageCompleted.HasValue) task.PercentageCompleted = dto.PercentageCompleted.Value;
        if (dto.AssignedTo != null) task.AssignedTo = dto.AssignedTo;
        if (dto.Status != null) task.Status = dto.Status;
        if (dto.Blockages != null) task.Blockages = dto.Blockages;
        if (dto.ProjectPhaseId.HasValue) task.ProjectPhaseId = dto.ProjectPhaseId.Value;
        
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Cargar ProjectPhase si existe
        if (task.ProjectPhaseId.HasValue)
        {
            await _context.Entry(task)
                .Reference(t => t.ProjectPhase)
                .LoadAsync();
        }

        // Recalcular porcentaje de la iteración
        await RecalculateIterationProgressAsync(task.IterationId);

        return Result<IterationTaskDto>.Ok(MapToTaskDto(task));
    }

    public async Task<Result<bool>> DeleteTaskAsync(int taskId)
    {
        var task = await _context.IterationTasks.FindAsync(taskId);
        if (task == null)
        {
            return Result<bool>.Fail("Tarea no encontrada");
        }

        var iterationId = task.IterationId;
        _context.IterationTasks.Remove(task);
        await _context.SaveChangesAsync();

        // Recalcular porcentaje de la iteración
        await RecalculateIterationProgressAsync(iterationId);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<ProjectProgressDto>> GetProjectProgressAsync(int projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Phases)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return Result<ProjectProgressDto>.Fail("Proyecto no encontrado");
        }

        var iterations = await _context.Iterations
            .Include(i => i.Tasks)
            .Where(i => i.ProjectId == projectId)
            .ToListAsync();

        // Calcular progreso general
        var overallPercentage = iterations.Any() 
            ? (int)iterations.Average(i => i.PercentageCompleted)
            : 0;

        // Calcular progreso por fase (simplificado - asumiendo distribución uniforme)
        var phaseProgress = project.Phases.OrderBy(p => p.Order).Select(phase => 
        {
            var phaseIterations = iterations.Count / project.Phases.Count;
            var phaseCompletedIterations = iterations
                .Where(i => i.PercentageCompleted == 100)
                .Count() / project.Phases.Count;

            return new PhaseProgressDto(
                phase.Id,
                phase.Name,
                phaseIterations > 0 ? (phaseCompletedIterations * 100 / phaseIterations) : 0,
                phaseIterations,
                phaseCompletedIterations
            );
        }).ToList();

        // Iteraciones recientes
        var recentIterations = iterations
            .OrderByDescending(i => i.StartDate)
            .Take(5)
            .Select(i => new IterationSummaryDto(
                i.Id,
                i.Name,
                i.StartDate,
                i.EndDate,
                i.PercentageCompleted,
                i.Tasks.Count,
                i.Tasks.Count(t => t.PercentageCompleted == 100)
            ))
            .ToList();

        var progressDto = new ProjectProgressDto(
            projectId,
            overallPercentage,
            phaseProgress,
            recentIterations
        );

        return Result<ProjectProgressDto>.Ok(progressDto);
    }

    private async Task RecalculateIterationProgressAsync(int iterationId)
    {
        var iteration = await _context.Iterations
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == iterationId);

        if (iteration != null && iteration.Tasks.Any())
        {
            iteration.PercentageCompleted = (int)iteration.Tasks.Average(t => t.PercentageCompleted);
            await _context.SaveChangesAsync();
        }
    }

    private static IterationDto MapToDto(Iteration iteration)
    {
        return new IterationDto(
            iteration.Id,
            iteration.ProjectId,
            iteration.Name,
            iteration.StartDate,
            iteration.EndDate,
            iteration.PercentageCompleted,
            iteration.Blockages,
            iteration.Observations,
            iteration.CreatedAt,
            iteration.UpdatedAt,
            iteration.Tasks?.Select(MapToTaskDto).ToList() ?? new List<IterationTaskDto>()
        );
    }

    private static IterationTaskDto MapToTaskDto(IterationTask task)
    {
        return new IterationTaskDto(
            task.Id,
            task.IterationId,
            task.ProjectPhaseId,
            task.ProjectPhase?.Name,
            task.Name,
            task.Description,
            task.StartDate,
            task.EndDate,
            task.PercentageCompleted,
            task.AssignedTo,
            task.Status,
            task.Blockages,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}
