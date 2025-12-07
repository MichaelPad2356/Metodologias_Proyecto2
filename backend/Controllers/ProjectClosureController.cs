using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

/// <summary>
/// HU-026: Controlador para cierre de proyecto y generación de documento de cierre
/// </summary>
[ApiController]
[Route("api/projects/{projectId}/closure")]
[Authorize]
public class ProjectClosureController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectClosureController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Valida si el proyecto puede cerrarse
    /// </summary>
    [HttpGet("validate")]
    public async Task<ActionResult<ClosureValidationResult>> ValidateClosure(int projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Phases)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound("Proyecto no encontrado");

        var result = await PerformClosureValidation(project);
        return Ok(result);
    }

    /// <summary>
    /// Ejecuta el cierre del proyecto
    /// </summary>
    [HttpPost("close")]
    public async Task<ActionResult<ProjectClosureDto>> CloseProject(int projectId, CloseProjectRequest request)
    {
        var project = await _context.Projects
            .Include(p => p.Phases)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound("Proyecto no encontrado");

        if (project.Status == ProjectStatus.Closed)
            return BadRequest("El proyecto ya está cerrado");

        // Validar cierre
        var validation = await PerformClosureValidation(project);

        // Si no puede cerrar y no es cierre forzado, rechazar
        if (!validation.CanClose && !request.ForceClose)
        {
            return BadRequest(new
            {
                message = "No se puede cerrar el proyecto. Hay requisitos pendientes.",
                validation = validation
            });
        }

        // Si es cierre forzado, validar justificación
        if (!validation.CanClose && request.ForceClose)
        {
            if (string.IsNullOrWhiteSpace(request.ForceCloseJustification))
                return BadRequest("Se requiere una justificación para el cierre forzado");
        }

        // Crear registro de cierre
        var closure = new ProjectClosure
        {
            ProjectId = projectId,
            ClosedBy = User.Identity?.Name ?? "Sistema",
            IsForcedClose = !validation.CanClose && request.ForceClose,
            ForceCloseJustification = request.ForceCloseJustification,
            ValidationResultJson = JsonSerializer.Serialize(validation),
            ArtifactsSummaryJson = await GenerateArtifactsSummary(projectId),
            TeamMembersJson = await GenerateTeamSummary(projectId)
        };

        _context.ProjectClosures.Add(closure);

        // Actualizar estado del proyecto
        project.Status = ProjectStatus.Closed;
        project.ArchivedAt = DateTime.UtcNow;

        // Crear artefacto de cierre automático si no existe
        await CreateClosureDocument(project, closure, validation);

        await _context.SaveChangesAsync();

        return Ok(new ProjectClosureDto
        {
            Id = closure.Id,
            ProjectId = projectId,
            ProjectName = project.Name,
            ClosedAt = closure.ClosedAt,
            ClosedBy = closure.ClosedBy,
            IsForcedClose = closure.IsForcedClose,
            ForceCloseJustification = closure.ForceCloseJustification,
            ValidationSummary = validation,
            Message = "Proyecto cerrado exitosamente. Se ha generado el Documento de Cierre."
        });
    }

    /// <summary>
    /// Obtiene el documento de cierre de un proyecto
    /// </summary>
    [HttpGet("document")]
    public async Task<ActionResult<ProjectClosureDocumentDto>> GetClosureDocument(int projectId)
    {
        var closure = await _context.ProjectClosures
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.ProjectId == projectId);

        if (closure == null)
            return NotFound("No se encontró documento de cierre para este proyecto");

        var validation = closure.ValidationResultJson != null
            ? JsonSerializer.Deserialize<ClosureValidationResult>(closure.ValidationResultJson)
            : null;

        var artifacts = closure.ArtifactsSummaryJson != null
            ? JsonSerializer.Deserialize<List<ArtifactSummaryItem>>(closure.ArtifactsSummaryJson)
            : new List<ArtifactSummaryItem>();

        var team = closure.TeamMembersJson != null
            ? JsonSerializer.Deserialize<List<TeamMemberSummary>>(closure.TeamMembersJson)
            : new List<TeamMemberSummary>();

        return Ok(new ProjectClosureDocumentDto
        {
            ProjectId = projectId,
            ProjectName = closure.Project?.Name ?? "",
            ProjectCode = closure.Project?.Code ?? "",
            ClosedAt = closure.ClosedAt,
            ClosedBy = closure.ClosedBy,
            IsForcedClose = closure.IsForcedClose,
            ForceCloseJustification = closure.ForceCloseJustification,
            ValidationResult = validation,
            Artifacts = artifacts ?? new List<ArtifactSummaryItem>(),
            TeamMembers = team ?? new List<TeamMemberSummary>()
        });
    }

    /// <summary>
    /// Reabre un proyecto cerrado
    /// </summary>
    [HttpPost("reopen")]
    public async Task<IActionResult> ReopenProject(int projectId, [FromBody] ReopenProjectRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound("Proyecto no encontrado");

        if (project.Status != ProjectStatus.Closed)
            return BadRequest("El proyecto no está cerrado");

        project.Status = ProjectStatus.Active;
        project.ArchivedAt = null;

        // Registrar en auditoría
        _context.AuditLogs.Add(new AuditLog
        {
            EntityType = "Project",
            EntityId = projectId,
            Action = "Reopen",
            Details = request.Reason ?? "Proyecto reabierto",
            UserName = User.Identity?.Name ?? "Sistema",
            Timestamp = DateTime.UtcNow,
            ProjectId = projectId
        });

        await _context.SaveChangesAsync();

        return Ok(new { message = "Proyecto reabierto exitosamente" });
    }

    #region Private Methods

    private async Task<ClosureValidationResult> PerformClosureValidation(Project project)
    {
        var result = new ClosureValidationResult
        {
            ProjectId = project.Id,
            ProjectName = project.Name
        };

        // 1. Verificar artefactos obligatorios
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == project.Id && p.Name.Contains("Transición"));

        if (transitionPhase != null)
        {
            var artifacts = await _context.Artifacts
                .Where(a => a.ProjectPhaseId == transitionPhase.Id)
                .ToListAsync();

            var mandatoryTypes = new Dictionary<ArtifactType, string>
            {
                { ArtifactType.UserManual, "Manual de Usuario" },
                { ArtifactType.TechnicalManual, "Manual Técnico" },
                { ArtifactType.DeploymentPlan, "Plan de Despliegue" },
                { ArtifactType.FinalBuild, "Build Final" }
            };

            foreach (var (type, name) in mandatoryTypes)
            {
                var artifact = artifacts.FirstOrDefault(a => a.Type == type);
                var item = new ClosureCheckItem
                {
                    Name = name,
                    IsRequired = true
                };

                if (artifact == null)
                {
                    item.Status = "Faltante";
                    item.IsPassed = false;
                }
                else if (artifact.Status != ArtifactStatus.Approved)
                {
                    item.Status = $"Pendiente de aprobación (Estado: {artifact.Status})";
                    item.IsPassed = false;
                }
                else if (!artifact.Versions.Any())
                {
                    item.Status = "Sin versión entregada";
                    item.IsPassed = false;
                }
                else
                {
                    item.Status = "Completado";
                    item.IsPassed = true;
                }

                result.Checklist.Add(item);
            }
        }
        else
        {
            result.Checklist.Add(new ClosureCheckItem
            {
                Name = "Fase de Transición",
                IsRequired = true,
                IsPassed = false,
                Status = "No encontrada"
            });
        }

        // 2. Verificar que todas las fases estén completadas
        var incompletedPhases = project.Phases.Where(p => p.Status != PhaseStatus.Completed).ToList();
        if (incompletedPhases.Any())
        {
            result.Checklist.Add(new ClosureCheckItem
            {
                Name = "Fases del proyecto completadas",
                IsRequired = false,
                IsPassed = false,
                Status = $"Fases pendientes: {string.Join(", ", incompletedPhases.Select(p => p.Name))}"
            });
        }
        else
        {
            result.Checklist.Add(new ClosureCheckItem
            {
                Name = "Fases del proyecto completadas",
                IsRequired = false,
                IsPassed = true,
                Status = "Todas las fases completadas"
            });
        }

        // 3. Verificar defectos abiertos
        var openDefects = await _context.Defects
            .Where(d => d.ProjectId == project.Id && d.Status != "Closed" && d.Status != "Resolved")
            .CountAsync();

        result.Checklist.Add(new ClosureCheckItem
        {
            Name = "Defectos cerrados",
            IsRequired = false,
            IsPassed = openDefects == 0,
            Status = openDefects == 0 ? "Todos los defectos cerrados" : $"{openDefects} defectos abiertos"
        });

        // Calcular resultado final
        result.CanClose = result.Checklist.Where(c => c.IsRequired).All(c => c.IsPassed);
        result.PassedItems = result.Checklist.Count(c => c.IsPassed);
        result.TotalItems = result.Checklist.Count;

        return result;
    }

    private async Task<string> GenerateArtifactsSummary(int projectId)
    {
        var phases = await _context.ProjectPhases
            .Where(p => p.ProjectId == projectId)
            .Include(p => p.Artifacts)
            .ThenInclude(a => a.Versions)
            .ToListAsync();

        var summary = new List<ArtifactSummaryItem>();

        foreach (var phase in phases)
        {
            foreach (var artifact in phase.Artifacts)
            {
                summary.Add(new ArtifactSummaryItem
                {
                    Type = artifact.Type.ToString(),
                    Phase = phase.Name,
                    Status = artifact.Status.ToString(),
                    VersionCount = artifact.Versions.Count,
                    IsMandatory = artifact.IsMandatory,
                    CreatedAt = artifact.CreatedAt
                });
            }
        }

        return JsonSerializer.Serialize(summary);
    }

    private async Task<string> GenerateTeamSummary(int projectId)
    {
        var members = await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId && m.Status == MemberStatus.Accepted)
            .Select(m => new TeamMemberSummary
            {
                Name = m.UserName ?? m.UserEmail,
                Email = m.UserEmail,
                Role = m.Role
            })
            .ToListAsync();

        return JsonSerializer.Serialize(members);
    }

    private async Task CreateClosureDocument(Project project, ProjectClosure closure, ClosureValidationResult validation)
    {
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == project.Id && p.Name.Contains("Transición"));

        if (transitionPhase == null) return;

        // Verificar si ya existe un documento de cierre
        var existingClosure = await _context.Artifacts
            .FirstOrDefaultAsync(a => a.ProjectPhaseId == transitionPhase.Id && a.Type == ArtifactType.ClosureDoc);

        if (existingClosure != null) return;

        // Crear documento de cierre automático
        var closureArtifact = new Artifact
        {
            Type = ArtifactType.ClosureDoc,
            ProjectPhaseId = transitionPhase.Id,
            IsMandatory = true,
            Status = ArtifactStatus.Approved,
            AssignedTo = closure.ClosedBy,
            ClosureChecklistJson = JsonSerializer.Serialize(validation.Checklist)
        };

        var version = new ArtifactVersion
        {
            Artifact = closureArtifact,
            VersionNumber = 1,
            Author = closure.ClosedBy ?? "Sistema",
            Content = $"Documento de Cierre generado automáticamente el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.\n\n" +
                     $"Proyecto: {project.Name}\n" +
                     $"Código: {project.Code}\n" +
                     $"Cerrado por: {closure.ClosedBy}\n" +
                     $"Cierre forzado: {(closure.IsForcedClose ? "Sí" : "No")}\n" +
                     (closure.IsForcedClose ? $"Justificación: {closure.ForceCloseJustification}\n" : "") +
                     $"\nValidación: {validation.PassedItems}/{validation.TotalItems} items completados"
        };

        closureArtifact.Versions.Add(version);
        _context.Artifacts.Add(closureArtifact);
    }

    #endregion
}

#region DTOs

public class CloseProjectRequest
{
    public bool ForceClose { get; set; } = false;
    public string? ForceCloseJustification { get; set; }
}

public class ReopenProjectRequest
{
    public string? Reason { get; set; }
}

public class ClosureValidationResult
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool CanClose { get; set; }
    public int PassedItems { get; set; }
    public int TotalItems { get; set; }
    public List<ClosureCheckItem> Checklist { get; set; } = new();
}

public class ClosureCheckItem
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsPassed { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProjectClosureDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
    public bool IsForcedClose { get; set; }
    public string? ForceCloseJustification { get; set; }
    public ClosureValidationResult? ValidationSummary { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ProjectClosureDocumentDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public DateTime ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
    public bool IsForcedClose { get; set; }
    public string? ForceCloseJustification { get; set; }
    public ClosureValidationResult? ValidationResult { get; set; }
    public List<ArtifactSummaryItem> Artifacts { get; set; } = new();
    public List<TeamMemberSummary> TeamMembers { get; set; } = new();
}

public class ArtifactSummaryItem
{
    public string Type { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int VersionCount { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TeamMemberSummary
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

#endregion
