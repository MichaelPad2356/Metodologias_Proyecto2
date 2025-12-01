using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Contracts;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ArtifactsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("phase/{phaseId}")]
    public async Task<ActionResult<IEnumerable<ArtifactDto>>> GetArtifactsForPhase(int phaseId)
    {
        var artifacts = await _context.Artifacts
            .Include(a => a.Workflow)
            .Include(a => a.CurrentStep)
            .Where(a => a.ProjectPhaseId == phaseId)
            .Select(a => new ArtifactDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Type = a.Type,
                Status = a.Status,
                IsMandatory = a.IsMandatory,
                WorkflowId = a.WorkflowId,
                WorkflowName = a.Workflow != null ? a.Workflow.Name : null,
                CurrentStepId = a.CurrentStepId,
                CurrentStepName = a.CurrentStep != null ? a.CurrentStep.Name : null,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return Ok(artifacts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArtifactDto>> GetArtifact(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(a => a.CurrentStep)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        var dto = new ArtifactDto
        {
            Id = artifact.Id,
            Name = artifact.Name,
            Description = artifact.Description,
            Type = artifact.Type,
            Status = artifact.Status,
            IsMandatory = artifact.IsMandatory,
            WorkflowId = artifact.WorkflowId,
            WorkflowName = artifact.Workflow?.Name,
            CurrentStepId = artifact.CurrentStepId,
            CurrentStepName = artifact.CurrentStep?.Name,
            CreatedAt = artifact.CreatedAt,
            UpdatedAt = artifact.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ArtifactDto>> CreateArtifact(CreateArtifactDto dto)
    {
        var artifact = new Artifact
        {
            ProjectPhaseId = dto.ProjectPhaseId,
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Status = ArtifactStatus.Pending,
            IsMandatory = dto.IsMandatory,
            WorkflowId = dto.WorkflowId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Si tiene workflow asignado, establecer el primer paso como CurrentStep
        if (dto.WorkflowId.HasValue)
        {
            var firstStep = await _context.WorkflowSteps
                .Where(s => s.WorkflowId == dto.WorkflowId.Value)
                .OrderBy(s => s.Order)
                .FirstOrDefaultAsync();

            if (firstStep != null)
            {
                artifact.CurrentStepId = firstStep.Id;
            }
        }

        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();

        // Cargar relaciones para el DTO de respuesta
        await _context.Entry(artifact)
            .Reference(a => a.Workflow)
            .LoadAsync();
        await _context.Entry(artifact)
            .Reference(a => a.CurrentStep)
            .LoadAsync();

        var resultDto = new ArtifactDto
        {
            Id = artifact.Id,
            Name = artifact.Name,
            Description = artifact.Description,
            Type = artifact.Type,
            Status = artifact.Status,
            IsMandatory = artifact.IsMandatory,
            WorkflowId = artifact.WorkflowId,
            WorkflowName = artifact.Workflow?.Name,
            CurrentStepId = artifact.CurrentStepId,
            CurrentStepName = artifact.CurrentStep?.Name,
            CreatedAt = artifact.CreatedAt,
            UpdatedAt = artifact.UpdatedAt
        };

        return CreatedAtAction(nameof(GetArtifact), new { id = artifact.Id }, resultDto);
    }

    [HttpPut("{id}/change-step")]
    public async Task<IActionResult> ChangeWorkflowStep(int id, [FromBody] ChangeStepDto dto)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(a => a.CurrentStep)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        if (artifact.WorkflowId == null)
            return BadRequest("Este artefacto no tiene un flujo de trabajo asignado");

        var newStep = await _context.WorkflowSteps
            .FirstOrDefaultAsync(s => s.Id == dto.NewStepId && s.WorkflowId == artifact.WorkflowId);

        if (newStep == null)
            return BadRequest("El paso seleccionado no pertenece al flujo de trabajo de este artefacto");

        // Registrar el cambio en el historial
        var history = new ArtifactHistory
        {
            ArtifactId = artifact.Id,
            PreviousState = artifact.CurrentStep?.Name ?? "Sin estado",
            NewState = newStep.Name,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = dto.ChangedBy ?? "Sistema",
            Comments = dto.Comments ?? string.Empty
        };

        artifact.CurrentStepId = dto.NewStepId;
        artifact.UpdatedAt = DateTime.UtcNow;

        _context.ArtifactHistories.Add(history);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<ArtifactHistory>>> GetArtifactHistory(int id)
    {
        var history = await _context.ArtifactHistories
            .Where(h => h.ArtifactId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        return Ok(history);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtifact(int id, CreateArtifactDto dto)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        artifact.Name = dto.Name;
        artifact.Description = dto.Description;
        artifact.Type = dto.Type;
        artifact.IsMandatory = dto.IsMandatory;
        artifact.WorkflowId = dto.WorkflowId;
        artifact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtifact(int id)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        _context.Artifacts.Remove(artifact);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ChangeStepDto
{
    public int NewStepId { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comments { get; set; }
}