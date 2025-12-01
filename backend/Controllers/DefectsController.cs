using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DefectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DefectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/defects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Defect>>> GetDefects([FromQuery] int? projectId)
    {
        var query = _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(d => d.ProjectId == projectId.Value);

        var defects = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Ok(defects);
    }

    // GET: api/defects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Defect>> GetDefect(int id)
    {
        var defect = await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (defect == null)
            return NotFound();

        return Ok(defect);
    }

    // POST: api/defects
    [HttpPost]
    public async Task<ActionResult<Defect>> CreateDefect(CreateDefectDto dto)
    {
        var project = await _context.Projects.FindAsync(dto.ProjectId);
        if (project == null)
            return BadRequest("Project not found");

        if (dto.ArtifactId.HasValue)
        {
            var artifact = await _context.Artifacts.FindAsync(dto.ArtifactId.Value);
            if (artifact == null)
                return BadRequest("Artifact not found");
        }

        var defect = new Defect
        {
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Status = DefectStatus.New,
            ProjectId = dto.ProjectId,
            ArtifactId = dto.ArtifactId,
            ReportedBy = dto.ReportedBy,
            AssignedTo = dto.AssignedTo,
            CreatedAt = DateTime.UtcNow
        };

        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDefect), new { id = defect.Id }, defect);
    }

    // PUT: api/defects/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDefect(int id, UpdateDefectDto dto)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
            return NotFound();

        if (dto.Title != null)
            defect.Title = dto.Title;
        if (dto.Description != null)
            defect.Description = dto.Description;
        if (dto.Severity.HasValue)
            defect.Severity = dto.Severity.Value;
        if (dto.Status.HasValue)
            defect.Status = dto.Status.Value;
        if (dto.AssignedTo != null)
            defect.AssignedTo = dto.AssignedTo;

        defect.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/defects/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDefect(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
            return NotFound();

        _context.Defects.Remove(defect);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs for Defects
public record CreateDefectDto(
    string Title,
    string? Description,
    DefectSeverity Severity,
    int ProjectId,
    int? ArtifactId,
    string? ReportedBy,
    string? AssignedTo
);

public record UpdateDefectDto(
    string? Title = null,
    string? Description = null,
    DefectSeverity? Severity = null,
    DefectStatus? Status = null,
    string? AssignedTo = null
);
