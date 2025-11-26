using backend.Contracts;
using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MicroincrementsController : ControllerBase
{
    private readonly IMicroincrementService _service;

    public MicroincrementsController(IMicroincrementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<MicroincrementDto>>> GetAll()
    {
        var microincrements = await _service.GetAllAsync();
        return Ok(microincrements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MicroincrementDto>> GetById(int id)
    {
        var microincrement = await _service.GetByIdAsync(id);
        if (microincrement == null)
            return NotFound();
        return Ok(microincrement);
    }

    [HttpGet("by-iteration/{phaseId}")]
    public async Task<ActionResult<List<MicroincrementDto>>> GetByIteration(int phaseId)
    {
        var microincrements = await _service.GetByIterationAsync(phaseId);
        return Ok(microincrements);
    }

    [HttpGet("by-deliverable/{deliverableId}")]
    public async Task<ActionResult<List<MicroincrementDto>>> GetByDeliverable(int deliverableId)
    {
        var microincrements = await _service.GetByDeliverableAsync(deliverableId);
        return Ok(microincrements);
    }

    [HttpGet("by-author/{author}")]
    public async Task<ActionResult<List<MicroincrementDto>>> GetByAuthor(string author)
    {
        var microincrements = await _service.GetByAuthorAsync(author);
        return Ok(microincrements);
    }

    [HttpPost]
    public async Task<ActionResult<MicroincrementDto>> Create(CreateMicroincrementDto dto)
    {
        var microincrement = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = microincrement.Id }, microincrement);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MicroincrementDto>> Update(int id, UpdateMicroincrementDto dto)
    {
        var microincrement = await _service.UpdateAsync(id, dto);
        if (microincrement == null)
            return NotFound();
        return Ok(microincrement);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}