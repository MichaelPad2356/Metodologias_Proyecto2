using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DefectsController : ControllerBase
{
    private readonly IDefectService _defectService;

    public DefectsController(IDefectService defectService)
    {
        _defectService = defectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DefectDto>>> GetAll([FromQuery] int? projectId)
    {
        var defects = await _defectService.GetAllAsync(projectId);
        return Ok(defects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DefectDto>> GetById(int id)
    {
        var defect = await _defectService.GetByIdAsync(id);
        if (defect == null) return NotFound();
        return Ok(defect);
    }

    [HttpPost]
    public async Task<ActionResult<DefectDto>> Create(CreateDefectDto dto)
    {
        var defect = await _defectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = defect.Id }, defect);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DefectDto>> Update(int id, UpdateDefectDto dto)
    {
        var defect = await _defectService.UpdateAsync(id, dto);
        if (defect == null) return NotFound();
        return Ok(defect);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _defectService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
