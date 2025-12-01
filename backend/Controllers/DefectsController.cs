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
        var query = _context.Defects.AsQueryable();
        
        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }
        
        return await query
            .Include(d => d.Artifact)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    // GET: api/defects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Defect>> GetDefect(int id)
    {
        var defect = await _context.Defects
            .Include(d => d.Artifact)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (defect == null)
        {
            return NotFound();
        }

        return defect;
    }

    // POST: api/defects
    [HttpPost]
    public async Task<ActionResult<Defect>> CreateDefect(Defect defect)
    {
        defect.CreatedAt = DateTime.UtcNow;
        defect.Status = DefectStatus.New;
        
        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDefect), new { id = defect.Id }, defect);
    }

    // PUT: api/defects/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDefect(int id, Defect defect)
    {
        if (id != defect.Id)
        {
            return BadRequest();
        }

        defect.UpdatedAt = DateTime.UtcNow;
        _context.Entry(defect).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DefectExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/defects/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDefect(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
        {
            return NotFound();
        }

        _context.Defects.Remove(defect);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DefectExists(int id)
    {
        return _context.Defects.Any(e => e.Id == id);
    }
}
