using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/projects/{projectId}/iterations")]
public class IterationsController : ControllerBase
{
    private readonly IIterationService _iterationService;
    private readonly ILogger<IterationsController> _logger;

    public IterationsController(
        IIterationService iterationService,
        ILogger<IterationsController> logger)
    {
        _iterationService = iterationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las iteraciones de un proyecto
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<IterationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectIterations(int projectId)
    {
        var result = await _iterationService.GetProjectIterationsAsync(projectId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una iteración por ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(IterationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIterationById(int id)
    {
        var result = await _iterationService.GetIterationByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Crea una nueva iteración
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IterationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIteration(int projectId, [FromBody] CreateIterationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _iterationService.CreateIterationAsync(projectId, dto);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Iteración creada: {IterationName} para proyecto {ProjectId}", result.Data!.Name, projectId);
        
        return CreatedAtAction(
            nameof(GetIterationById),
            new { projectId, id = result.Data.Id },
            result.Data
        );
    }

    /// <summary>
    /// Actualiza una iteración
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(IterationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIteration(int id, [FromBody] UpdateIterationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _iterationService.UpdateIterationAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Iteración actualizada: ID {IterationId}", id);
        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina una iteración
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIteration(int id)
    {
        var result = await _iterationService.DeleteIterationAsync(id);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Iteración eliminada: ID {IterationId}", id);
        return Ok(new { message = "Iteración eliminada exitosamente", success = true });
    }

    /// <summary>
    /// Crea una tarea en una iteración
    /// </summary>
    [HttpPost("{iterationId:int}/tasks")]
    [ProducesResponseType(typeof(IterationTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(int iterationId, [FromBody] CreateIterationTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _iterationService.CreateTaskAsync(iterationId, dto);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Tarea creada: {TaskName} en iteración {IterationId}", result.Data!.Name, iterationId);
        
        return CreatedAtAction(
            nameof(GetIterationById),
            new { projectId = 0, id = iterationId },
            result.Data
        );
    }

    /// <summary>
    /// Actualiza una tarea
    /// </summary>
    [HttpPut("{iterationId:int}/tasks/{taskId:int}")]
    [ProducesResponseType(typeof(IterationTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateIterationTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _iterationService.UpdateTaskAsync(taskId, dto);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Tarea actualizada: ID {TaskId}", taskId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    [HttpDelete("{iterationId:int}/tasks/{taskId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var result = await _iterationService.DeleteTaskAsync(taskId);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Tarea eliminada: ID {TaskId}", taskId);
        return Ok(new { message = "Tarea eliminada exitosamente", success = true });
    }
}

[ApiController]
[Route("api/projects/{projectId}/progress")]
public class ProjectProgressController : ControllerBase
{
    private readonly IIterationService _iterationService;

    public ProjectProgressController(IIterationService iterationService)
    {
        _iterationService = iterationService;
    }

    /// <summary>
    /// Obtiene el resumen de progreso del proyecto
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProjectProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectProgress(int projectId)
    {
        var result = await _iterationService.GetProjectProgressAsync(projectId);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}
