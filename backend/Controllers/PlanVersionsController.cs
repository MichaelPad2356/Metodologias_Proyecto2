using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/projects/{projectId}/plan-versions")]
[Authorize]
public class PlanVersionsController : ControllerBase
{
    private readonly IPlanVersionService _planVersionService;

    public PlanVersionsController(IPlanVersionService planVersionService)
    {
        _planVersionService = planVersionService;
    }

    /// <summary>
    /// Guarda la versión actual del plan del proyecto como una nueva versión histórica
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PlanVersionDto>> SavePlanVersion(
        int projectId,
        [FromBody] SavePlanVersionDto dto)
    {
        try
        {
            var userName = User.Identity?.Name ?? "System";
            var version = await _planVersionService.SavePlanVersionAsync(projectId, dto, userName);
            return CreatedAtAction(
                nameof(GetPlanVersion),
                new { projectId = projectId, version = version.Version },
                version);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al guardar la versión del plan", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las versiones históricas del plan de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanVersionDto>>> GetPlanVersions(int projectId)
    {
        try
        {
            var versions = await _planVersionService.GetPlanVersionsAsync(projectId);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las versiones del plan", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una versión específica del plan de un proyecto
    /// </summary>
    [HttpGet("{version}")]
    public async Task<ActionResult<PlanVersionDto>> GetPlanVersion(int projectId, int version)
    {
        try
        {
            var planVersion = await _planVersionService.GetPlanVersionAsync(projectId, version);
            if (planVersion == null)
            {
                return NotFound(new { message = $"Versión {version} del plan no encontrada para el proyecto {projectId}" });
            }
            return Ok(planVersion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la versión del plan", error = ex.Message });
        }
    }
}
