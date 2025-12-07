using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

/// <summary>
/// HU-018: Controlador para configuración global del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ConfigurationController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Roles del Sistema

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<SystemRole>>> GetRoles()
    {
        return await _context.SystemRoles.OrderBy(r => r.Name).ToListAsync();
    }

    [HttpGet("roles/{id}")]
    public async Task<ActionResult<SystemRole>> GetRole(int id)
    {
        var role = await _context.SystemRoles.FindAsync(id);
        if (role == null) return NotFound();
        return role;
    }

    [HttpPost("roles")]
    public async Task<ActionResult<SystemRole>> CreateRole(SystemRoleDto dto)
    {
        var role = new SystemRole
        {
            Name = dto.Name,
            Description = dto.Description,
            PermissionsJson = dto.PermissionsJson,
            IsSystem = false
        };

        _context.SystemRoles.Add(role);
        await LogConfigChange("Role", 0, "Create", null, role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    [HttpPut("roles/{id}")]
    public async Task<IActionResult> UpdateRole(int id, SystemRoleDto dto)
    {
        var role = await _context.SystemRoles.FindAsync(id);
        if (role == null) return NotFound();

        var oldValues = JsonSerializer.Serialize(role);

        role.Name = dto.Name;
        role.Description = dto.Description;
        role.PermissionsJson = dto.PermissionsJson;
        role.UpdatedAt = DateTime.UtcNow;

        await LogConfigChange("Role", id, "Update", oldValues, role);
        await _context.SaveChangesAsync();

        return Ok(role);
    }

    [HttpDelete("roles/{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.SystemRoles.FindAsync(id);
        if (role == null) return NotFound();
        if (role.IsSystem) return BadRequest("No se puede eliminar un rol del sistema");

        var oldValues = JsonSerializer.Serialize(role);
        await LogConfigChange("Role", id, "Delete", oldValues, null);

        _context.SystemRoles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Tipos de Artefactos Personalizados

    [HttpGet("artifact-types")]
    public async Task<ActionResult<IEnumerable<CustomArtifactType>>> GetArtifactTypes()
    {
        return await _context.CustomArtifactTypes.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
    }

    [HttpPost("artifact-types")]
    public async Task<ActionResult<CustomArtifactType>> CreateArtifactType(CustomArtifactTypeDto dto)
    {
        var artifactType = new CustomArtifactType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultPhase = dto.DefaultPhase,
            IsMandatoryByDefault = dto.IsMandatoryByDefault,
            CustomFieldsJson = dto.CustomFieldsJson
        };

        _context.CustomArtifactTypes.Add(artifactType);
        await LogConfigChange("ArtifactType", 0, "Create", null, artifactType);
        await _context.SaveChangesAsync();

        return Ok(artifactType);
    }

    [HttpPut("artifact-types/{id}")]
    public async Task<IActionResult> UpdateArtifactType(int id, CustomArtifactTypeDto dto)
    {
        var artifactType = await _context.CustomArtifactTypes.FindAsync(id);
        if (artifactType == null) return NotFound();

        var oldValues = JsonSerializer.Serialize(artifactType);

        artifactType.Name = dto.Name;
        artifactType.Description = dto.Description;
        artifactType.DefaultPhase = dto.DefaultPhase;
        artifactType.IsMandatoryByDefault = dto.IsMandatoryByDefault;
        artifactType.CustomFieldsJson = dto.CustomFieldsJson;

        await LogConfigChange("ArtifactType", id, "Update", oldValues, artifactType);
        await _context.SaveChangesAsync();

        return Ok(artifactType);
    }

    [HttpDelete("artifact-types/{id}")]
    public async Task<IActionResult> DeleteArtifactType(int id)
    {
        var artifactType = await _context.CustomArtifactTypes.FindAsync(id);
        if (artifactType == null) return NotFound();

        var oldValues = JsonSerializer.Serialize(artifactType);
        artifactType.IsActive = false; // Soft delete
        
        await LogConfigChange("ArtifactType", id, "Delete", oldValues, null);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Fases Personalizadas

    [HttpGet("phases")]
    public async Task<ActionResult<IEnumerable<CustomPhaseDefinition>>> GetPhaseDefinitions()
    {
        return await _context.CustomPhaseDefinitions.Where(p => p.IsActive).OrderBy(p => p.Order).ToListAsync();
    }

    [HttpPost("phases")]
    public async Task<ActionResult<CustomPhaseDefinition>> CreatePhaseDefinition(CustomPhaseDefinitionDto dto)
    {
        var phase = new CustomPhaseDefinition
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            MandatoryArtifactTypesJson = dto.MandatoryArtifactTypesJson
        };

        _context.CustomPhaseDefinitions.Add(phase);
        await LogConfigChange("Phase", 0, "Create", null, phase);
        await _context.SaveChangesAsync();

        return Ok(phase);
    }

    [HttpPut("phases/{id}")]
    public async Task<IActionResult> UpdatePhaseDefinition(int id, CustomPhaseDefinitionDto dto)
    {
        var phase = await _context.CustomPhaseDefinitions.FindAsync(id);
        if (phase == null) return NotFound();

        var oldValues = JsonSerializer.Serialize(phase);

        phase.Name = dto.Name;
        phase.Description = dto.Description;
        phase.Order = dto.Order;
        phase.MandatoryArtifactTypesJson = dto.MandatoryArtifactTypesJson;

        await LogConfigChange("Phase", id, "Update", oldValues, phase);
        await _context.SaveChangesAsync();

        return Ok(phase);
    }

    [HttpDelete("phases/{id}")]
    public async Task<IActionResult> DeletePhaseDefinition(int id)
    {
        var phase = await _context.CustomPhaseDefinitions.FindAsync(id);
        if (phase == null) return NotFound();

        var oldValues = JsonSerializer.Serialize(phase);
        phase.IsActive = false;

        await LogConfigChange("Phase", id, "Delete", oldValues, null);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Historial de Configuración

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ConfigurationHistory>>> GetConfigurationHistory(
        [FromQuery] string? entityType = null,
        [FromQuery] int? limit = 50)
    {
        var query = _context.ConfigurationHistory.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(h => h.EntityType == entityType);

        return await query.OrderByDescending(h => h.ChangedAt).Take(limit ?? 50).ToListAsync();
    }

    [HttpPost("revert/{historyId}")]
    public async Task<IActionResult> RevertConfiguration(int historyId)
    {
        var history = await _context.ConfigurationHistory.FindAsync(historyId);
        if (history == null) return NotFound("Registro de historial no encontrado");

        if (string.IsNullOrEmpty(history.OldValuesJson))
            return BadRequest("No hay valores anteriores para revertir");

        // Revertir según el tipo de entidad
        switch (history.EntityType)
        {
            case "Role":
                var oldRole = JsonSerializer.Deserialize<SystemRole>(history.OldValuesJson);
                if (oldRole != null)
                {
                    var existingRole = await _context.SystemRoles.FindAsync(history.EntityId);
                    if (existingRole != null)
                    {
                        existingRole.Name = oldRole.Name;
                        existingRole.Description = oldRole.Description;
                        existingRole.PermissionsJson = oldRole.PermissionsJson;
                    }
                    else
                    {
                        oldRole.Id = 0;
                        _context.SystemRoles.Add(oldRole);
                    }
                }
                break;

            case "ArtifactType":
                var oldArtifactType = JsonSerializer.Deserialize<CustomArtifactType>(history.OldValuesJson);
                if (oldArtifactType != null)
                {
                    var existing = await _context.CustomArtifactTypes.FindAsync(history.EntityId);
                    if (existing != null)
                    {
                        existing.Name = oldArtifactType.Name;
                        existing.Description = oldArtifactType.Description;
                        existing.IsActive = true;
                    }
                }
                break;

            case "Phase":
                var oldPhase = JsonSerializer.Deserialize<CustomPhaseDefinition>(history.OldValuesJson);
                if (oldPhase != null)
                {
                    var existing = await _context.CustomPhaseDefinitions.FindAsync(history.EntityId);
                    if (existing != null)
                    {
                        existing.Name = oldPhase.Name;
                        existing.Description = oldPhase.Description;
                        existing.Order = oldPhase.Order;
                        existing.IsActive = true;
                    }
                }
                break;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Configuración revertida exitosamente" });
    }

    #endregion

    private async Task LogConfigChange(string entityType, int entityId, string action, string? oldValues, object? newEntity)
    {
        var history = new ConfigurationHistory
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValuesJson = oldValues,
            NewValuesJson = newEntity != null ? JsonSerializer.Serialize(newEntity) : null,
            ChangedBy = User.Identity?.Name ?? "Sistema"
        };

        _context.ConfigurationHistory.Add(history);
    }
}

#region DTOs

public class SystemRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PermissionsJson { get; set; }
}

public class CustomArtifactTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultPhase { get; set; }
    public bool IsMandatoryByDefault { get; set; }
    public string? CustomFieldsJson { get; set; }
}

public class CustomPhaseDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string? MandatoryArtifactTypesJson { get; set; }
}

#endregion
