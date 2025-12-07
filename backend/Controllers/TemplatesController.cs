using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

/// <summary>
/// HU-019: Controlador para gestión de plantillas OpenUP
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TemplatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las plantillas activas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OpenUpTemplateListDto>>> GetTemplates()
    {
        var templates = await _context.OpenUpTemplates
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new OpenUpTemplateListDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Version = t.Version,
                IsDefault = t.IsDefault,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                ParentTemplateId = t.ParentTemplateId
            })
            .ToListAsync();

        return Ok(templates);
    }

    /// <summary>
    /// Obtiene una plantilla completa por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OpenUpTemplate>> GetTemplate(int id)
    {
        var template = await _context.OpenUpTemplates.FindAsync(id);
        if (template == null) return NotFound();
        return template;
    }

    /// <summary>
    /// Crea una nueva plantilla OpenUP
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OpenUpTemplate>> CreateTemplate(CreateTemplateDto dto)
    {
        // Si es la primera plantilla o se marca como default, asegurarse que sea única
        if (dto.IsDefault)
        {
            var existingDefault = await _context.OpenUpTemplates.Where(t => t.IsDefault).ToListAsync();
            foreach (var t in existingDefault)
            {
                t.IsDefault = false;
            }
        }

        var template = new OpenUpTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            Version = 1,
            IsDefault = dto.IsDefault,
            PhasesJson = dto.PhasesJson ?? GetDefaultPhasesJson(),
            RolesJson = dto.RolesJson ?? GetDefaultRolesJson(),
            ArtifactTypesJson = dto.ArtifactTypesJson ?? GetDefaultArtifactTypesJson(),
            WorkflowsJson = dto.WorkflowsJson ?? "[]",
            CreatedBy = User.Identity?.Name ?? "Sistema",
            ParentTemplateId = dto.ParentTemplateId
        };

        _context.OpenUpTemplates.Add(template);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    /// <summary>
    /// Crea una nueva versión de una plantilla existente
    /// </summary>
    [HttpPost("{id}/new-version")]
    public async Task<ActionResult<OpenUpTemplate>> CreateNewVersion(int id, CreateTemplateDto dto)
    {
        var parentTemplate = await _context.OpenUpTemplates.FindAsync(id);
        if (parentTemplate == null) return NotFound("Plantilla padre no encontrada");

        var latestVersion = await _context.OpenUpTemplates
            .Where(t => t.ParentTemplateId == id || t.Id == id)
            .MaxAsync(t => t.Version);

        var newTemplate = new OpenUpTemplate
        {
            Name = dto.Name ?? $"{parentTemplate.Name} v{latestVersion + 1}",
            Description = dto.Description ?? parentTemplate.Description,
            Version = latestVersion + 1,
            IsDefault = false,
            PhasesJson = dto.PhasesJson ?? parentTemplate.PhasesJson,
            RolesJson = dto.RolesJson ?? parentTemplate.RolesJson,
            ArtifactTypesJson = dto.ArtifactTypesJson ?? parentTemplate.ArtifactTypesJson,
            WorkflowsJson = dto.WorkflowsJson ?? parentTemplate.WorkflowsJson,
            CreatedBy = User.Identity?.Name ?? "Sistema",
            ParentTemplateId = id
        };

        _context.OpenUpTemplates.Add(newTemplate);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTemplate), new { id = newTemplate.Id }, newTemplate);
    }

    /// <summary>
    /// Actualiza una plantilla
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTemplate(int id, UpdateTemplateDto dto)
    {
        var template = await _context.OpenUpTemplates.FindAsync(id);
        if (template == null) return NotFound();

        if (dto.Name != null) template.Name = dto.Name;
        if (dto.Description != null) template.Description = dto.Description;
        if (dto.PhasesJson != null) template.PhasesJson = dto.PhasesJson;
        if (dto.RolesJson != null) template.RolesJson = dto.RolesJson;
        if (dto.ArtifactTypesJson != null) template.ArtifactTypesJson = dto.ArtifactTypesJson;
        if (dto.WorkflowsJson != null) template.WorkflowsJson = dto.WorkflowsJson;

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            var existingDefaults = await _context.OpenUpTemplates.Where(t => t.IsDefault && t.Id != id).ToListAsync();
            foreach (var t in existingDefaults)
            {
                t.IsDefault = false;
            }
            template.IsDefault = true;
        }

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(template);
    }

    /// <summary>
    /// Compara dos plantillas
    /// </summary>
    [HttpGet("compare")]
    public async Task<ActionResult<TemplateComparisonDto>> CompareTemplates(
        [FromQuery] int templateId1,
        [FromQuery] int templateId2)
    {
        var template1 = await _context.OpenUpTemplates.FindAsync(templateId1);
        var template2 = await _context.OpenUpTemplates.FindAsync(templateId2);

        if (template1 == null || template2 == null)
            return NotFound("Una o ambas plantillas no fueron encontradas");

        var comparison = new TemplateComparisonDto
        {
            Template1 = new TemplateMetadataDto
            {
                Id = template1.Id,
                Name = template1.Name,
                Version = template1.Version,
                CreatedAt = template1.CreatedAt
            },
            Template2 = new TemplateMetadataDto
            {
                Id = template2.Id,
                Name = template2.Name,
                Version = template2.Version,
                CreatedAt = template2.CreatedAt
            },
            Differences = new List<string>()
        };

        // Comparar fases
        if (template1.PhasesJson != template2.PhasesJson)
            comparison.Differences.Add("Las fases son diferentes");

        // Comparar roles
        if (template1.RolesJson != template2.RolesJson)
            comparison.Differences.Add("Los roles son diferentes");

        // Comparar tipos de artefactos
        if (template1.ArtifactTypesJson != template2.ArtifactTypesJson)
            comparison.Differences.Add("Los tipos de artefactos son diferentes");

        // Comparar workflows
        if (template1.WorkflowsJson != template2.WorkflowsJson)
            comparison.Differences.Add("Los flujos de trabajo son diferentes");

        if (!comparison.Differences.Any())
            comparison.Differences.Add("Las plantillas son idénticas en contenido");

        return Ok(comparison);
    }

    /// <summary>
    /// Elimina una plantilla (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        var template = await _context.OpenUpTemplates.FindAsync(id);
        if (template == null) return NotFound();

        if (template.IsDefault)
            return BadRequest("No se puede eliminar la plantilla por defecto");

        template.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtiene la plantilla por defecto
    /// </summary>
    [HttpGet("default")]
    public async Task<ActionResult<OpenUpTemplate>> GetDefaultTemplate()
    {
        var template = await _context.OpenUpTemplates.FirstOrDefaultAsync(t => t.IsDefault && t.IsActive);
        if (template == null)
        {
            // Crear plantilla por defecto si no existe
            template = new OpenUpTemplate
            {
                Name = "OpenUP Estándar",
                Description = "Plantilla estándar de OpenUP con las 4 fases tradicionales",
                Version = 1,
                IsDefault = true,
                PhasesJson = GetDefaultPhasesJson(),
                RolesJson = GetDefaultRolesJson(),
                ArtifactTypesJson = GetDefaultArtifactTypesJson(),
                CreatedBy = "Sistema"
            };

            _context.OpenUpTemplates.Add(template);
            await _context.SaveChangesAsync();
        }

        return template;
    }

    #region Helpers

    private string GetDefaultPhasesJson()
    {
        var phases = new[]
        {
            new { Name = "Inicio", Order = 1, Description = "Fase de concepción del proyecto" },
            new { Name = "Elaboración", Order = 2, Description = "Fase de arquitectura y diseño" },
            new { Name = "Construcción", Order = 3, Description = "Fase de desarrollo e implementación" },
            new { Name = "Transición", Order = 4, Description = "Fase de despliegue y cierre" }
        };
        return JsonSerializer.Serialize(phases);
    }

    private string GetDefaultRolesJson()
    {
        var roles = new[]
        {
            new { Name = "Product Owner", Permissions = new[] { "create", "edit", "approve", "close" } },
            new { Name = "Scrum Master", Permissions = new[] { "create", "edit", "manage_team" } },
            new { Name = "Desarrollador", Permissions = new[] { "create", "edit" } },
            new { Name = "Tester", Permissions = new[] { "create", "edit", "review" } },
            new { Name = "Revisor", Permissions = new[] { "review", "approve" } },
            new { Name = "Administrador", Permissions = new[] { "all" } }
        };
        return JsonSerializer.Serialize(roles);
    }

    private string GetDefaultArtifactTypesJson()
    {
        var artifactTypes = new[]
        {
            new { Name = "Documento de Visión", Phase = "Inicio", IsMandatory = true },
            new { Name = "Lista de Interesados", Phase = "Inicio", IsMandatory = true },
            new { Name = "Lista de Riesgos", Phase = "Inicio", IsMandatory = true },
            new { Name = "Documento de Arquitectura", Phase = "Elaboración", IsMandatory = true },
            new { Name = "Modelo de Casos de Uso", Phase = "Elaboración", IsMandatory = true },
            new { Name = "Código Fuente", Phase = "Construcción", IsMandatory = true },
            new { Name = "Casos de Prueba", Phase = "Construcción", IsMandatory = false },
            new { Name = "Manual de Usuario", Phase = "Transición", IsMandatory = true },
            new { Name = "Plan de Despliegue", Phase = "Transición", IsMandatory = true }
        };
        return JsonSerializer.Serialize(artifactTypes);
    }

    #endregion
}

#region DTOs

public class OpenUpTemplateListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsDefault { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ParentTemplateId { get; set; }
}

public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public string? PhasesJson { get; set; }
    public string? RolesJson { get; set; }
    public string? ArtifactTypesJson { get; set; }
    public string? WorkflowsJson { get; set; }
    public int? ParentTemplateId { get; set; }
}

public class UpdateTemplateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDefault { get; set; }
    public string? PhasesJson { get; set; }
    public string? RolesJson { get; set; }
    public string? ArtifactTypesJson { get; set; }
    public string? WorkflowsJson { get; set; }
}

public class TemplateComparisonDto
{
    public TemplateMetadataDto Template1 { get; set; } = new();
    public TemplateMetadataDto Template2 { get; set; } = new();
    public List<string> Differences { get; set; } = new();
}

public class TemplateMetadataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
