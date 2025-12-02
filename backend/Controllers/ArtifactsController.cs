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
    [HttpGet("{id:int}")]
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

    // GET: api/artifacts/phase/5 - Get artifacts by phase
    [HttpGet("phase/{phaseId}")]
    public async Task<ActionResult<IEnumerable<ArtifactDto>>> GetArtifactsByPhase(int phaseId)
    {
        var artifacts = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .Where(a => a.ProjectPhaseId == phaseId)
            .ToListAsync();

        return Ok(artifacts.Select(a => MapToDto(a)));
    }

    // GET: api/artifacts/transition/2 - Get transition phase artifacts for a project
    [HttpGet("transition/{projectId}")]
    public async Task<ActionResult<TransitionArtifactsResponse>> GetTransitionArtifacts(int projectId)
    {
        // Get the transition phase for the project (phase 4 = Transition in OpenUP)
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Name.Contains("Transición"));

        if (transitionPhase == null)
        {
            // Try to find by order (transition is typically phase 4)
            transitionPhase = await _context.ProjectPhases
                .Where(p => p.ProjectId == projectId)
                .OrderBy(p => p.Order)
                .Skip(3) // Skip first 3 phases (Inception, Elaboration, Construction)
                .FirstOrDefaultAsync();
        }

        // Define mandatory artifact types for transition phase
        var mandatoryTypes = new List<ArtifactTypeInfo>
        {
            new(ArtifactType.UserManual, "Manual de Usuario"),
            new(ArtifactType.TechnicalManual, "Manual Técnico"),
            new(ArtifactType.DeploymentPlan, "Plan de Despliegue"),
            new(ArtifactType.ClosureDocument, "Documento de Cierre"),
            new(ArtifactType.FinalBuild, "Build Final")
        };

        if (transitionPhase == null)
        {
            return Ok(new TransitionArtifactsResponse(
                0,
                new List<ArtifactDto>(),
                mandatoryTypes,
                mandatoryTypes, // All are missing
                false
            ));
        }

        var artifacts = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .Where(a => a.ProjectPhaseId == transitionPhase.Id)
            .ToListAsync();

        var artifactDtos = artifacts.Select(a => MapToDto(a)).ToList();
        var existingTypes = artifacts.Select(a => a.Type).ToHashSet();
        
        var missingMandatory = mandatoryTypes
            .Where(m => !existingTypes.Contains(m.Type))
            .ToList();

        var canClose = !missingMandatory.Any() && 
                       artifacts.All(a => a.Status == ArtifactStatus.Approved);

        return Ok(new TransitionArtifactsResponse(
            transitionPhase.Id,
            artifactDtos,
            mandatoryTypes,
            missingMandatory,
            canClose
        ));
    }

    // POST: api/artifacts/validate-closure/2 - Validate if project can be closed
    [HttpPost("validate-closure/{projectId}")]
    public async Task<ActionResult<ClosureValidationResponse>> ValidateProjectClosure(int projectId)
    {
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Name.Contains("Transición"));

        if (transitionPhase == null)
        {
            transitionPhase = await _context.ProjectPhases
                .Where(p => p.ProjectId == projectId)
                .OrderBy(p => p.Order)
                .Skip(3)
                .FirstOrDefaultAsync();
        }

        var mandatoryTypes = new[] 
        { 
            ArtifactType.UserManual, 
            ArtifactType.TechnicalManual, 
            ArtifactType.DeploymentPlan, 
            ArtifactType.ClosureDocument, 
            ArtifactType.FinalBuild 
        };

        var missingArtifacts = new List<string>();
        var pendingApproval = new List<ArtifactTypeInfo>();

        if (transitionPhase == null)
        {
            missingArtifacts.AddRange(new[] { "Manual de Usuario", "Manual Técnico", "Plan de Despliegue", "Documento de Cierre", "Build Final" });
            return Ok(new ClosureValidationResponse(
                false,
                missingArtifacts,
                pendingApproval,
                new ChecklistValidation(false, new List<string> { "No hay fase de transición configurada" })
            ));
        }

        var artifacts = await _context.Artifacts
            .Where(a => a.ProjectPhaseId == transitionPhase.Id)
            .ToListAsync();

        var existingTypes = artifacts.Select(a => a.Type).ToHashSet();

        foreach (var type in mandatoryTypes)
        {
            if (!existingTypes.Contains(type))
            {
                missingArtifacts.Add(GetArtifactTypeName(type));
            }
            else
            {
                var artifact = artifacts.First(a => a.Type == type);
                if (artifact.Status != ArtifactStatus.Approved)
                {
                    pendingApproval.Add(new ArtifactTypeInfo(type, GetArtifactTypeName(type)));
                }
            }
        }

        var pendingChecklistItems = new List<string>();
        var closureDoc = artifacts.FirstOrDefault(a => a.Type == ArtifactType.ClosureDocument);
        if (closureDoc != null && !string.IsNullOrEmpty(closureDoc.ClosureChecklistJson))
        {
            // Parse checklist and find uncompleted items
            try
            {
                var checklist = System.Text.Json.JsonSerializer.Deserialize<List<ChecklistItem>>(closureDoc.ClosureChecklistJson);
                if (checklist != null)
                {
                    pendingChecklistItems = checklist.Where(c => !c.Completed).Select(c => c.Label).ToList();
                }
            }
            catch { /* Ignore parse errors */ }
        }

        var canClose = !missingArtifacts.Any() && !pendingApproval.Any() && !pendingChecklistItems.Any();

        return Ok(new ClosureValidationResponse(
            canClose,
            missingArtifacts,
            pendingApproval,
            new ChecklistValidation(!pendingChecklistItems.Any(), pendingChecklistItems)
        ));
    }

    private string GetArtifactTypeName(ArtifactType type) => type switch
    {
        ArtifactType.UserManual => "Manual de Usuario",
        ArtifactType.TechnicalManual => "Manual Técnico",
        ArtifactType.DeploymentPlan => "Plan de Despliegue",
        ArtifactType.ClosureDocument => "Documento de Cierre",
        ArtifactType.FinalBuild => "Build Final",
        ArtifactType.BetaTestReport => "Reporte de Pruebas Beta",
        _ => type.ToString()
    };

    // POST: api/artifacts
    [HttpPost]
    public async Task<ActionResult<ArtifactDto>> CreateArtifact(
        [FromForm] int projectPhaseId,
        [FromForm] int type,
        [FromForm] string author,
        [FromForm] bool isMandatory = true,
        [FromForm] string? content = null,
        [FromForm] string? observations = null,
        [FromForm] string? buildIdentifier = null,
        [FromForm] string? buildDownloadUrl = null,
        [FromForm] string? closureChecklistJson = null,
        IFormFile? file = null)
    {
        var phase = await _context.ProjectPhases.FindAsync(projectPhaseId);
        if (phase == null)
            return BadRequest("ProjectPhase not found");

        var artifactType = (ArtifactType)type;

        var artifact = new Artifact
        {
            Type = artifactType,
            ProjectPhaseId = projectPhaseId,
            IsMandatory = isMandatory,
            Status = ArtifactStatus.Pending,
            BuildIdentifier = buildIdentifier,
            BuildDownloadUrl = buildDownloadUrl,
            ClosureChecklistJson = closureChecklistJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();

        // Create first version
        var version = new ArtifactVersion
        {
            ArtifactId = artifact.Id,
            VersionNumber = 1,
            Author = author,
            Observations = observations ?? "Versión inicial",
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
        await _context.SaveChangesAsync();

        // Reload artifact with versions
        artifact = await _context.Artifacts
            .Include(a => a.ProjectPhase)
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);

        return CreatedAtAction(nameof(GetArtifact), new { id = artifact!.Id }, MapToDto(artifact));
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
