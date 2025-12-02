using backend.Contracts;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ArtifactsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/artifacts/phase/{phaseId}
    [HttpGet("phase/{phaseId}")]
    public async Task<IActionResult> GetArtifactsForPhase(int phaseId)
    {
        var artifacts = await _context.Artifacts
            .Where(a => a.ProjectPhaseId == phaseId)
            .Include(a => a.Versions)
            .Select(a => new ArtifactDto
            {
                Id = a.Id,
                Type = a.Type,
                ProjectPhaseId = a.ProjectPhaseId,
                IsMandatory = a.IsMandatory,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                Versions = a.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new ArtifactVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    Author = v.Author,
                    Content = v.Content,
                    OriginalFileName = v.OriginalFileName,
                    RepositoryUrl = v.RepositoryUrl,
                    CreatedAt = v.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
                }).ToList()
            })
            .ToListAsync();

        return Ok(artifacts);
    }

    // GET: api/artifacts/project/{projectId}
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetArtifactsByProject(int projectId)
    {
        var artifacts = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Where(a => a.ProjectPhase.ProjectId == projectId)
            .Include(a => a.Versions)
            .Select(a => new ArtifactDto
            {
                Id = a.Id,
                Type = a.Type,
                ProjectPhaseId = a.ProjectPhaseId,
                IsMandatory = a.IsMandatory,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                Versions = a.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new ArtifactVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    Author = v.Author,
                    Content = v.Content,
                    OriginalFileName = v.OriginalFileName,
                    RepositoryUrl = v.RepositoryUrl,
                    CreatedAt = v.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
                }).ToList()
            })
            .ToListAsync();

        return Ok(artifacts);
    }

    // POST: api/artifacts
    [HttpPost]
    public async Task<IActionResult> CreateArtifact([FromForm] CreateArtifactDto dto)
    {
        var phase = await _context.ProjectPhases.FindAsync(dto.ProjectPhaseId);
        if (phase == null)
        {
            return NotFound("Project phase not found.");
        }

        var artifact = new Artifact
        {
            Type = dto.Type,
            ProjectPhaseId = dto.ProjectPhaseId,
            IsMandatory = dto.IsMandatory,
            Status = ArtifactStatus.Pending,
            AssignedTo = dto.AssignedTo
        };

        var firstVersion = new ArtifactVersion
        {
            Artifact = artifact,
            VersionNumber = 1,
            Author = dto.Author,
            Content = dto.Content,
            RepositoryUrl = dto.RepositoryUrl
        };

        if (dto.File != null)
        {
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            firstVersion.FilePath = filePath;
            firstVersion.OriginalFileName = dto.File.FileName;
            firstVersion.ContentType = dto.File.ContentType;
        }

        artifact.Versions.Add(firstVersion);
        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();

        var resultDto = new ArtifactDto
        {
            Id = artifact.Id,
            Type = artifact.Type,
            ProjectPhaseId = artifact.ProjectPhaseId,
            IsMandatory = artifact.IsMandatory,
            Status = artifact.Status,
            CreatedAt = artifact.CreatedAt,
            Versions = new List<ArtifactVersionDto> { 
                new ArtifactVersionDto {
                    Id = firstVersion.Id,
                    VersionNumber = firstVersion.VersionNumber,
                    Author = firstVersion.Author,
                    Content = firstVersion.Content,
                    OriginalFileName = firstVersion.OriginalFileName,
                    RepositoryUrl = firstVersion.RepositoryUrl,
                    CreatedAt = firstVersion.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(firstVersion.FilePath) ? $"/uploads/{Path.GetFileName(firstVersion.FilePath)}" : null
                }
            }
        };

        return CreatedAtAction(nameof(GetArtifactsForPhase), new { phaseId = artifact.ProjectPhaseId }, resultDto);
    }

    // POST: api/artifacts/{id}/versions
    [HttpPost("{id}/versions")]
    public async Task<IActionResult> AddVersion(int id, [FromForm] CreateArtifactVersionDto dto)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null) return NotFound("Artifact not found.");

        var nextVersionNumber = artifact.Versions.Any() 
            ? artifact.Versions.Max(v => v.VersionNumber) + 1 
            : 1;

        var version = new ArtifactVersion
        {
            ArtifactId = id,
            VersionNumber = nextVersionNumber,
            Author = dto.Author,
            Content = dto.Content,
            RepositoryUrl = dto.RepositoryUrl,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.File != null)
        {
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            version.FilePath = filePath;
            version.OriginalFileName = dto.File.FileName;
            version.ContentType = dto.File.ContentType;
        }

        _context.ArtifactVersions.Add(version);
        
        // Reset status to Pending when a new version is uploaded (Workflow logic)
        artifact.Status = ArtifactStatus.Pending;
        
        await _context.SaveChangesAsync();

        var versionDto = new ArtifactVersionDto
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Author = version.Author,
            Content = version.Content,
            OriginalFileName = version.OriginalFileName,
            RepositoryUrl = version.RepositoryUrl,
            CreatedAt = version.CreatedAt,
            DownloadUrl = !string.IsNullOrEmpty(version.FilePath) ? $"/uploads/{Path.GetFileName(version.FilePath)}" : null
        };

        return Ok(versionDto);
    }

    // PUT: api/artifacts/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateArtifactStatus(int id, [FromBody] string status)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null) return NotFound();

        if (Enum.TryParse<ArtifactStatus>(status, true, out var statusEnum))
        {
            artifact.Status = statusEnum;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Estado actualizado" });
        }
        return BadRequest("Estado inválido");
    }
}