<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
=======
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d

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
<<<<<<< HEAD
    public async Task<ActionResult<IEnumerable<Defect>>> GetDefects([FromQuery] int? projectId)
    {
        var query = _context.Defects.AsQueryable();
        
=======
    public async Task<IActionResult> GetDefects([FromQuery] int? projectId)
    {
        var query = _context.Defects.AsQueryable();

>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }
<<<<<<< HEAD
        
        return await query
            .Include(d => d.Artifact)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    // GET: api/defects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Defect>> GetDefect(int id)
=======

        var defects = await query
            .Include(d => d.Artifact)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DefectDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                Severity = d.Severity.ToString(),
                Status = d.Status.ToString(),
                ProjectId = d.ProjectId,
                ArtifactId = d.ArtifactId,
                ReportedBy = d.ReportedBy,
                AssignedTo = d.AssignedTo,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return Ok(defects);
    }

    // GET: api/defects/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDefect(int id)
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
    {
        var defect = await _context.Defects
            .Include(d => d.Artifact)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (defect == null)
        {
<<<<<<< HEAD
            return NotFound();
        }

        return defect;
=======
            return NotFound("Defect not found.");
        }

        return Ok(new DefectDto
        {
            Id = defect.Id,
            Title = defect.Title,
            Description = defect.Description,
            Severity = defect.Severity.ToString(),
            Status = defect.Status.ToString(),
            ProjectId = defect.ProjectId,
            ArtifactId = defect.ArtifactId,
            ReportedBy = defect.ReportedBy,
            AssignedTo = defect.AssignedTo,
            CreatedAt = defect.CreatedAt
        });
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
    }

    // POST: api/defects
    [HttpPost]
<<<<<<< HEAD
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
=======
    public async Task<IActionResult> CreateDefect([FromBody] CreateDefectDto dto)
    {
        // Verificar que el proyecto existe
        var project = await _context.Projects.FindAsync(dto.ProjectId);
        if (project == null)
        {
            return BadRequest("Project not found.");
        }

        // Parsear severidad
        if (!Enum.TryParse<DefectSeverity>(dto.Severity, true, out var severity))
        {
            severity = DefectSeverity.Medium;
        }

        var defect = new Defect
        {
            Title = dto.Title,
            Description = dto.Description,
            Severity = severity,
            Status = DefectStatus.New,
            ProjectId = dto.ProjectId,
            ArtifactId = dto.ArtifactId,
            ReportedBy = dto.ReportedBy,
            AssignedTo = dto.AssignedTo
        };

        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDefect), new { id = defect.Id }, new DefectDto
        {
            Id = defect.Id,
            Title = defect.Title,
            Description = defect.Description,
            Severity = defect.Severity.ToString(),
            Status = defect.Status.ToString(),
            ProjectId = defect.ProjectId,
            ArtifactId = defect.ArtifactId,
            ReportedBy = defect.ReportedBy,
            AssignedTo = defect.AssignedTo,
            CreatedAt = defect.CreatedAt
        });
    }

    // PUT: api/defects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDefect(int id, [FromBody] UpdateDefectDto dto)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
        {
            return NotFound("Defect not found.");
        }

        if (!string.IsNullOrEmpty(dto.Title))
        {
            defect.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            defect.Description = dto.Description;
        }

        if (!string.IsNullOrEmpty(dto.Severity) && Enum.TryParse<DefectSeverity>(dto.Severity, true, out var severity))
        {
            defect.Severity = severity;
        }

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<DefectStatus>(dto.Status, true, out var status))
        {
            defect.Status = status;
        }

        if (dto.AssignedTo != null)
        {
            defect.AssignedTo = dto.AssignedTo;
        }

        defect.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new DefectDto
        {
            Id = defect.Id,
            Title = defect.Title,
            Description = defect.Description,
            Severity = defect.Severity.ToString(),
            Status = defect.Status.ToString(),
            ProjectId = defect.ProjectId,
            ArtifactId = defect.ArtifactId,
            ReportedBy = defect.ReportedBy,
            AssignedTo = defect.AssignedTo,
            CreatedAt = defect.CreatedAt
        });
    }

    // DELETE: api/defects/{id}
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDefect(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
        {
<<<<<<< HEAD
            return NotFound();
=======
            return NotFound("Defect not found.");
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
        }

        _context.Defects.Remove(defect);
        await _context.SaveChangesAsync();

        return NoContent();
    }
<<<<<<< HEAD

    private bool DefectExists(int id)
    {
        return _context.Defects.Any(e => e.Id == id);
    }
=======
}

// DTOs
public class DefectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "New";
    public int ProjectId { get; set; }
    public int? ArtifactId { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDefectDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
    public int ProjectId { get; set; }
    public int? ArtifactId { get; set; }
    public string? ReportedBy { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateDefectDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
}
