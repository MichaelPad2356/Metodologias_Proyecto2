using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Contracts;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ArtifactsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: api/artifacts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArtifactDto>>> GetArtifacts([FromQuery] int? phaseId, [FromQuery] ArtifactType? type)
    {
        var query = _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .AsQueryable();

        if (phaseId.HasValue)
            query = query.Where(a => a.ProjectPhaseId == phaseId.Value);

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        var artifacts = await query.ToListAsync();

        return Ok(artifacts.Select(a => MapToDto(a)));
    }

    // GET: api/artifacts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ArtifactDto>> GetArtifact(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        return Ok(MapToDto(artifact));
    }

    // POST: api/artifacts
    [HttpPost]
    public async Task<ActionResult<ArtifactDto>> CreateArtifact(CreateArtifactDto dto)
    {
        var phase = await _context.ProjectPhases.FindAsync(dto.ProjectPhaseId);
        if (phase == null)
            return BadRequest("ProjectPhase not found");

        var artifact = new Artifact
        {
            Type = dto.Type,
            ProjectPhaseId = dto.ProjectPhaseId,
            IsMandatory = dto.IsMandatory,
            Status = dto.Status,
            BuildIdentifier = dto.BuildIdentifier,
            BuildDownloadUrl = dto.BuildDownloadUrl,
            ClosureChecklistJson = dto.ClosureChecklistJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArtifact), new { id = artifact.Id }, MapToDto(artifact));
    }

    // PUT: api/artifacts/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ArtifactStatus status)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        artifact.Status = status;
        artifact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // HU-009: PUT api/artifacts/5/build-info
    [HttpPut("{id}/build-info")]
    public async Task<IActionResult> UpdateBuildInfo(int id, UpdateBuildInfoDto dto)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        artifact.BuildIdentifier = dto.BuildIdentifier;
        artifact.BuildDownloadUrl = dto.BuildDownloadUrl;
        artifact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // HU-009: PUT api/artifacts/5/closure-checklist
    [HttpPut("{id}/closure-checklist")]
    public async Task<IActionResult> UpdateClosureChecklist(int id, UpdateClosureChecklistDto dto)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        artifact.ClosureChecklistJson = dto.ClosureChecklistJson;
        artifact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/artifacts/5/versions
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<ArtifactVersionDto>>> GetVersions(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        var versions = artifact.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => MapVersionToDto(v));

        return Ok(versions);
    }

    // POST: api/artifacts/5/versions
    [HttpPost("{id}/versions")]
    public async Task<ActionResult<ArtifactVersionDto>> CreateVersion(int id, [FromForm] string author, [FromForm] string? observations, [FromForm] string? content, IFormFile? file)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        var maxVersion = artifact.Versions.Any() ? artifact.Versions.Max(v => v.VersionNumber) : 0;

        var version = new ArtifactVersion
        {
            ArtifactId = id,
            VersionNumber = maxVersion + 1,
            Author = author,
            Observations = observations,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        if (file != null)
        {
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            version.FilePath = filePath;
            version.OriginalFileName = file.FileName;
            version.ContentType = file.ContentType;
            version.FileSize = file.Length;
        }

        _context.ArtifactVersions.Add(version);
        artifact.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVersion), new { id, versionId = version.Id }, MapVersionToDto(version));
    }

    // GET: api/artifacts/5/versions/1
    [HttpGet("{id}/versions/{versionId}")]
    public async Task<ActionResult<ArtifactVersionDto>> GetVersion(int id, int versionId)
    {
        var version = await _context.ArtifactVersions
            .FirstOrDefaultAsync(v => v.ArtifactId == id && v.Id == versionId);

        if (version == null)
            return NotFound();

        return Ok(MapVersionToDto(version));
    }

    // GET: api/artifacts/5/versions/1/download
    [HttpGet("{id}/versions/{versionId}/download")]
    public async Task<IActionResult> DownloadVersion(int id, int versionId)
    {
        var version = await _context.ArtifactVersions
            .FirstOrDefaultAsync(v => v.ArtifactId == id && v.Id == versionId);

        if (version == null)
            return NotFound();

        if (string.IsNullOrEmpty(version.FilePath) || !System.IO.File.Exists(version.FilePath))
            return NotFound("File not found");

        var memory = new MemoryStream();
        using (var stream = new FileStream(version.FilePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        return File(memory, version.ContentType ?? "application/octet-stream", version.OriginalFileName);
    }

    // HU-010: GET api/artifacts/5/versions/compare?v1=1&v2=2
    [HttpGet("{id}/versions/compare")]
    public async Task<ActionResult<VersionComparisonDto>> CompareVersions(int id, [FromQuery] int v1, [FromQuery] int v2)
    {
        var version1 = await _context.ArtifactVersions
            .FirstOrDefaultAsync(v => v.ArtifactId == id && v.Id == v1);
        var version2 = await _context.ArtifactVersions
            .FirstOrDefaultAsync(v => v.ArtifactId == id && v.Id == v2);

        if (version1 == null || version2 == null)
            return NotFound("One or both versions not found");

        var changes = new List<string>();

        // Compare basic properties
        if (version1.Author != version2.Author)
            changes.Add($"Author changed from '{version1.Author}' to '{version2.Author}'");

        if (version1.Observations != version2.Observations)
            changes.Add($"Observations changed");

        if (version1.OriginalFileName != version2.OriginalFileName)
            changes.Add($"File changed from '{version1.OriginalFileName}' to '{version2.OriginalFileName}'");

        if (version1.FileSize != version2.FileSize)
            changes.Add($"File size changed from {version1.FileSize ?? 0} to {version2.FileSize ?? 0} bytes");

        if (version1.ContentType != version2.ContentType)
            changes.Add($"Content type changed from '{version1.ContentType}' to '{version2.ContentType}'");

        // Compare text content if available
        if (!string.IsNullOrEmpty(version1.Content) && !string.IsNullOrEmpty(version2.Content))
        {
            if (version1.Content != version2.Content)
            {
                var lines1 = version1.Content.Split('\n').Length;
                var lines2 = version2.Content.Split('\n').Length;
                changes.Add($"Content changed: {lines1} lines → {lines2} lines");
            }
        }

        if (!changes.Any())
            changes.Add("No significant changes detected");

        return Ok(new VersionComparisonDto(
            MapVersionToDto(version1),
            MapVersionToDto(version2),
            changes
        ));
    }

    // HU-010: GET api/artifacts/5/versions/export
    [HttpGet("{id}/versions/export")]
    public async Task<ActionResult<VersionHistoryExportDto>> ExportVersionHistory(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        var export = new VersionHistoryExportDto(
            artifact.Id,
            artifact.Type.ToString(),
            artifact.ProjectPhase?.Name ?? "Unknown",
            DateTime.UtcNow,
            artifact.Versions
                .OrderBy(v => v.VersionNumber)
                .Select(v => MapVersionToDto(v))
                .ToList()
        );

        return Ok(export);
    }

    // DELETE: api/artifacts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtifact(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        // Delete associated files
        foreach (var version in artifact.Versions)
        {
            if (!string.IsNullOrEmpty(version.FilePath) && System.IO.File.Exists(version.FilePath))
            {
                System.IO.File.Delete(version.FilePath);
            }
        }

        _context.Artifacts.Remove(artifact);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private ArtifactDto MapToDto(Artifact artifact)
    {
        var latestVersion = artifact.Versions
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();

        return new ArtifactDto(
            artifact.Id,
            artifact.Type,
            artifact.ProjectPhaseId,
            artifact.ProjectPhase?.Name,
            artifact.IsMandatory,
            artifact.Status,
            artifact.CreatedAt,
            artifact.UpdatedAt,
            artifact.Versions.Count,
            latestVersion != null ? MapVersionToDto(latestVersion) : null,
            artifact.BuildIdentifier,
            artifact.BuildDownloadUrl,
            artifact.ClosureChecklistJson
        );
    }

    private ArtifactVersionDto MapVersionToDto(ArtifactVersion version)
    {
        return new ArtifactVersionDto(
            version.Id,
            version.ArtifactId,
            version.VersionNumber,
            version.Author,
            version.Observations,
            version.Content,
            version.FilePath,
            version.OriginalFileName,
            version.ContentType,
            version.FileSize,
            version.CreatedAt
        );
    }
}
