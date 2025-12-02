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

        // Create first version
        var version = new ArtifactVersion
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