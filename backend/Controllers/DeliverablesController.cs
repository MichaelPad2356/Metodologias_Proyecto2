using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

/// <summary>
/// HU-020: Controlador para reasignación de entregables entre fases
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliverablesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DeliverablesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los entregables de una fase
    /// </summary>
    [HttpGet("phase/{phaseId}")]
    public async Task<ActionResult<IEnumerable<DeliverableDto>>> GetDeliverablesByPhase(int phaseId)
    {
        var deliverables = await _context.Deliverables
            .Where(d => d.ProjectPhaseId == phaseId)
            .Include(d => d.ProjectPhase)
            .Select(d => new DeliverableDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ProjectPhaseId = d.ProjectPhaseId,
                PhaseName = d.ProjectPhase!.Name,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return Ok(deliverables);
    }

    /// <summary>
    /// Obtiene un entregable por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DeliverableDto>> GetDeliverable(int id)
    {
        var deliverable = await _context.Deliverables
            .Include(d => d.ProjectPhase)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deliverable == null) return NotFound();

        return new DeliverableDto
        {
            Id = deliverable.Id,
            Name = deliverable.Name,
            Description = deliverable.Description,
            ProjectPhaseId = deliverable.ProjectPhaseId,
            PhaseName = deliverable.ProjectPhase?.Name ?? "",
            CreatedAt = deliverable.CreatedAt
        };
    }

    /// <summary>
    /// Mueve un entregable a otra fase con validación y registro
    /// </summary>
    [HttpPost("{id}/move")]
    public async Task<ActionResult<MoveDeliverableResponse>> MoveDeliverable(int id, MoveDeliverableDto dto)
    {
        var deliverable = await _context.Deliverables
            .Include(d => d.ProjectPhase)
            .Include(d => d.Microincrements)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deliverable == null)
            return NotFound("Entregable no encontrado");

        var fromPhase = deliverable.ProjectPhase;
        var toPhase = await _context.ProjectPhases
            .Include(p => p.Artifacts)
            .FirstOrDefaultAsync(p => p.Id == dto.ToPhaseId);

        if (toPhase == null)
            return NotFound("Fase destino no encontrada");

        if (fromPhase?.Id == toPhase.Id)
            return BadRequest("El entregable ya está en esa fase");

        // Validar reglas de negocio
        var warnings = await ValidateMoveRules(deliverable, fromPhase!, toPhase);
        var response = new MoveDeliverableResponse
        {
            DeliverableId = id,
            FromPhaseName = fromPhase?.Name ?? "",
            ToPhaseName = toPhase.Name,
            Warnings = warnings,
            RequiresConfirmation = warnings.Any()
        };

        // Si hay advertencias y no se confirmó, retornar para confirmación
        if (warnings.Any() && !dto.Confirmed)
        {
            response.Success = false;
            response.Message = "Se requiere confirmación para mover el entregable debido a las advertencias.";
            return Ok(response);
        }

        // Registrar el movimiento
        var movement = new DeliverableMovement
        {
            DeliverableId = id,
            FromPhaseId = fromPhase?.Id ?? 0,
            ToPhaseId = dto.ToPhaseId,
            Reason = dto.Reason,
            MovedBy = User.Identity?.Name ?? "Sistema",
            RequiredConfirmation = warnings.Any(),
            WarningsJson = warnings.Any() ? JsonSerializer.Serialize(warnings) : null
        };

        _context.DeliverableMovements.Add(movement);

        // Realizar el movimiento
        deliverable.ProjectPhaseId = dto.ToPhaseId;

        await _context.SaveChangesAsync();

        response.Success = true;
        response.Message = "Entregable movido exitosamente";
        response.MovementId = movement.Id;

        return Ok(response);
    }

    /// <summary>
    /// Obtiene el historial de movimientos de un entregable
    /// </summary>
    [HttpGet("{id}/movements")]
    public async Task<ActionResult<IEnumerable<DeliverableMovementDto>>> GetMovementHistory(int id)
    {
        var movementsRaw = await _context.DeliverableMovements
            .Where(m => m.DeliverableId == id)
            .OrderByDescending(m => m.MovedAt)
            .Select(m => new 
            {
                m.Id,
                m.FromPhaseId,
                m.ToPhaseId,
                m.Reason,
                m.MovedBy,
                m.MovedAt,
                m.RequiredConfirmation,
                m.WarningsJson
            })
            .ToListAsync();

        var movements = movementsRaw.Select(m => new DeliverableMovementDto
        {
            Id = m.Id,
            FromPhaseId = m.FromPhaseId,
            ToPhaseId = m.ToPhaseId,
            Reason = m.Reason,
            MovedBy = m.MovedBy,
            MovedAt = m.MovedAt,
            RequiredConfirmation = m.RequiredConfirmation,
            Warnings = !string.IsNullOrEmpty(m.WarningsJson)
                ? JsonSerializer.Deserialize<List<string>>(m.WarningsJson) ?? new List<string>()
                : new List<string>()
        }).ToList();

        // Agregar nombres de fases
        var phaseIds = movements.SelectMany(m => new[] { m.FromPhaseId, m.ToPhaseId }).Distinct();
        var phases = await _context.ProjectPhases
            .Where(p => phaseIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        foreach (var m in movements)
        {
            m.FromPhaseName = phases.ContainsKey(m.FromPhaseId) ? phases[m.FromPhaseId] : "Desconocida";
            m.ToPhaseName = phases.ContainsKey(m.ToPhaseId) ? phases[m.ToPhaseId] : "Desconocida";
        }

        return Ok(movements);
    }

    /// <summary>
    /// Crea un nuevo entregable
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DeliverableDto>> CreateDeliverable(CreateDeliverableDto dto)
    {
        var phase = await _context.ProjectPhases.FindAsync(dto.ProjectPhaseId);
        if (phase == null)
            return BadRequest("Fase no encontrada");

        var deliverable = new Deliverable
        {
            Name = dto.Name,
            Description = dto.Description,
            ProjectPhaseId = dto.ProjectPhaseId
        };

        _context.Deliverables.Add(deliverable);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDeliverable), new { id = deliverable.Id }, new DeliverableDto
        {
            Id = deliverable.Id,
            Name = deliverable.Name,
            Description = deliverable.Description,
            ProjectPhaseId = deliverable.ProjectPhaseId,
            PhaseName = phase.Name,
            CreatedAt = deliverable.CreatedAt
        });
    }

    #region Validation Helpers

    private async Task<List<string>> ValidateMoveRules(Deliverable deliverable, ProjectPhase fromPhase, ProjectPhase toPhase)
    {
        var warnings = new List<string>();

        // Regla 1: Advertir si se mueve hacia atrás en el orden de fases
        if (toPhase.Order < fromPhase.Order)
        {
            warnings.Add($"Está moviendo el entregable hacia una fase anterior ({toPhase.Name} está antes que {fromPhase.Name})");
        }

        // Regla 2: Advertir si la fase destino es Transición y faltan artefactos obligatorios
        if (toPhase.Name.Contains("Transición"))
        {
            var mandatoryArtifactsMissing = await CheckMandatoryArtifacts(toPhase.ProjectId);
            if (mandatoryArtifactsMissing.Any())
            {
                warnings.Add($"La fase de Transición tiene artefactos obligatorios pendientes: {string.Join(", ", mandatoryArtifactsMissing)}");
            }
        }

        // Regla 3: Advertir si el entregable tiene microincrementos asociados
        if (deliverable.Microincrements?.Any() == true)
        {
            warnings.Add($"Este entregable tiene {deliverable.Microincrements.Count} microincrementos asociados que también se moverán");
        }

        return warnings;
    }

    private async Task<List<string>> CheckMandatoryArtifacts(int projectId)
    {
        // Verificar artefactos obligatorios que faltan
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Name.Contains("Transición"));

        if (transitionPhase == null) return new List<string>();

        var existingArtifacts = await _context.Artifacts
            .Where(a => a.ProjectPhaseId == transitionPhase.Id)
            .Select(a => a.Type)
            .ToListAsync();

        var mandatoryTypes = new[] { ArtifactType.UserManual, ArtifactType.TechnicalManual, ArtifactType.DeploymentPlan };
        var missing = mandatoryTypes.Where(t => !existingArtifacts.Contains(t)).Select(t => t.ToString()).ToList();

        return missing;
    }

    #endregion
}

#region DTOs

public class DeliverableDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectPhaseId { get; set; }
    public string PhaseName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDeliverableDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectPhaseId { get; set; }
}

public class MoveDeliverableDto
{
    public int ToPhaseId { get; set; }
    public string? Reason { get; set; }
    public bool Confirmed { get; set; } = false;
}

public class MoveDeliverableResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DeliverableId { get; set; }
    public string FromPhaseName { get; set; } = string.Empty;
    public string ToPhaseName { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public bool RequiresConfirmation { get; set; }
    public int? MovementId { get; set; }
}

public class DeliverableMovementDto
{
    public int Id { get; set; }
    public int FromPhaseId { get; set; }
    public string FromPhaseName { get; set; } = string.Empty;
    public int ToPhaseId { get; set; }
    public string ToPhaseName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? MovedBy { get; set; }
    public DateTime MovedAt { get; set; }
    public bool RequiredConfirmation { get; set; }
    public List<string> Warnings { get; set; } = new();
}

#endregion
