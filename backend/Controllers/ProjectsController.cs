using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IPdfExportService _pdfExportService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectService projectService, 
        IPdfExportService pdfExportService,
        ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _pdfExportService = pdfExportService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los proyectos
    /// </summary>
    /// <param name="includeArchived">Incluir proyectos archivados</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjects([FromQuery] bool includeArchived = false)
    {
        var result = await _projectService.GetAllProjectsAsync(includeArchived);
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene un proyecto por ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var result = await _projectService.GetProjectByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene un proyecto por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectByCode(string code)
    {
        var result = await _projectService.GetProjectByCodeAsync(code);
        
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene las fases de un proyecto
    /// </summary>
    [HttpGet("{id:int}/phases")]
    [ProducesResponseType(typeof(List<ProjectPhaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectPhases(int id)
    {
        var result = await _projectService.GetProjectByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data!.Phases);
    }

    /// <summary>
    /// Crea un nuevo proyecto OpenUP con las 4 fases predeterminadas
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userName = User.Identity?.Name ?? "Sistema";
        var result = await _projectService.CreateProjectAsync(dto, userName);

        if (!result.Success)
        {
            _logger.LogWarning("Error al crear proyecto: {Error}", result.ErrorMessage);
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Proyecto creado: {ProjectName} ({ProjectCode})", result.Data!.Name, result.Data.Code);
        
        return CreatedAtAction(
            nameof(GetProjectById),
            new { id = result.Data.Id },
            result.Data
        );
    }

    /// <summary>
    /// Actualiza un proyecto existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userName = User.Identity?.Name ?? "Sistema";
        var result = await _projectService.UpdateProjectAsync(id, dto, userName);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Proyecto actualizado: ID {ProjectId}", id);
        return Ok(result.Data);
    }

    /// <summary>
    /// Archiva un proyecto (conserva el historial)
    /// </summary>
    [HttpPost("{id:int}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveProject(int id)
    {
        var userName = User.Identity?.Name ?? "Sistema";
        var result = await _projectService.ArchiveProjectAsync(id, userName);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Proyecto archivado: ID {ProjectId} por {UserName}", id, userName);
        return Ok(new { message = "Proyecto archivado exitosamente", success = true });
    }

    /// <summary>
    /// Desarchiva un proyecto
    /// </summary>
    [HttpPost("{id:int}/unarchive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnarchiveProject(int id)
    {
        var userName = User.Identity?.Name ?? "Sistema";
        var result = await _projectService.UnarchiveProjectAsync(id, userName);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Proyecto desarchivado: ID {ProjectId} por {UserName}", id, userName);
        return Ok(new { message = "Proyecto desarchivado exitosamente", success = true });
    }

    /// <summary>
    /// Elimina permanentemente un proyecto
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userName = User.Identity?.Name ?? "Sistema";
        var result = await _projectService.DeleteProjectAsync(id, userName);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogWarning("Proyecto eliminado permanentemente: ID {ProjectId} por {UserName}", id, userName);
        return Ok(new { message = "Proyecto eliminado exitosamente", success = true });
    }

    /// <summary>
    /// Exporta el plan del proyecto a PDF
    /// </summary>
    [HttpGet("{id:int}/export-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ExportProjectPlanToPdf(int id)
    {
        try
        {
            var pdfBytes = _pdfExportService.GenerateProjectPlanPdf(id);
            
            return File(pdfBytes, "application/pdf", $"Plan_Proyecto_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error al exportar PDF del proyecto {ProjectId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al exportar PDF del proyecto {ProjectId}", id);
            // Devuelve la excepción completa temporalmente para diagnóstico en desarrollo
            return StatusCode(500, new { message = "Error al generar el PDF", detail = ex.ToString() });
        }
    }
}
