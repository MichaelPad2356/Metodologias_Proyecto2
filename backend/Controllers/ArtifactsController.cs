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
            Status = ArtifactStatus.Pending
        };

        var firstVersion = new ArtifactVersion
        {
            Artifact = artifact,
            VersionNumber = 1,
            Author = dto.Author,
            Content = dto.Content
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
                    CreatedAt = firstVersion.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(firstVersion.FilePath) ? $"/uploads/{Path.GetFileName(firstVersion.FilePath)}" : null
                }
            }
        };

        return CreatedAtAction(nameof(GetArtifactsForPhase), new { phaseId = artifact.ProjectPhaseId }, resultDto);
    }
}