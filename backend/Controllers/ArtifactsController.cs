using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
=======
using backend.Data;
using backend.Models;
using backend.Contracts;
>>>>>>> origin/feature/-entregable

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
<<<<<<< HEAD
    private readonly IWebHostEnvironment _env;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
=======
>>>>>>> origin/feature/-entregable

    public ArtifactsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("phase/{phaseId}")]
    public async Task<ActionResult<IEnumerable<ArtifactDto>>> GetArtifactsForPhase(int phaseId)
    {
        var artifacts = await _context.Artifacts
            .Include(a => a.Workflow)
            .Include(a => a.CurrentStep)
            .Where(a => a.ProjectPhaseId == phaseId)
            .Select(a => new ArtifactDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Type = a.Type,
                Status = a.Status,
                IsMandatory = a.IsMandatory,
                WorkflowId = a.WorkflowId,
                WorkflowName = a.Workflow != null ? a.Workflow.Name : null,
                CurrentStepId = a.CurrentStepId,
                CurrentStepName = a.CurrentStep != null ? a.CurrentStep.Name : null,
                CreatedAt = a.CreatedAt,
<<<<<<< HEAD
                BuildIdentifier = a.BuildIdentifier,
                BuildDownloadUrl = a.BuildDownloadUrl,
                ClosureChecklistJson = a.ClosureChecklistJson,
                Versions = a.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new ArtifactVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    Author = v.Author,
                    Observations = v.Observations,  // HU-010
                    Content = v.Content,
                    OriginalFileName = v.OriginalFileName,
                    ContentType = v.ContentType,
                    FileSize = v.FileSize,
                    CreatedAt = v.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
                }).ToList()
=======
                UpdatedAt = a.UpdatedAt
>>>>>>> origin/feature/-entregable
            })
            .ToListAsync();

        return Ok(artifacts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArtifactDto>> GetArtifact(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(a => a.CurrentStep)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        var dto = new ArtifactDto
        {
            Id = artifact.Id,
            Name = artifact.Name,
            Description = artifact.Description,
            Type = artifact.Type,
            Status = artifact.Status,
            IsMandatory = artifact.IsMandatory,
            WorkflowId = artifact.WorkflowId,
            WorkflowName = artifact.Workflow?.Name,
            CurrentStepId = artifact.CurrentStepId,
            CurrentStepName = artifact.CurrentStep?.Name,
            CreatedAt = artifact.CreatedAt,
            UpdatedAt = artifact.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ArtifactDto>> CreateArtifact(CreateArtifactDto dto)
    {
        var artifact = new Artifact
        {
            ProjectPhaseId = dto.ProjectPhaseId,
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Status = ArtifactStatus.Pending,
            IsMandatory = dto.IsMandatory,
<<<<<<< HEAD
            Status = ArtifactStatus.Pending,
            BuildIdentifier = dto.BuildIdentifier,
            BuildDownloadUrl = dto.BuildDownloadUrl,
            ClosureChecklistJson = dto.ClosureChecklistJson
=======
            WorkflowId = dto.WorkflowId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
>>>>>>> origin/feature/-entregable
        };

        // Si tiene workflow asignado, establecer el primer paso como CurrentStep
        if (dto.WorkflowId.HasValue)
        {
<<<<<<< HEAD
            Artifact = artifact,
            VersionNumber = 1,
            Author = dto.Author,
            Observations = dto.Observations ?? "Versión inicial",  // HU-010
            Content = dto.Content
        };
=======
            var firstStep = await _context.WorkflowSteps
                .Where(s => s.WorkflowId == dto.WorkflowId.Value)
                .OrderBy(s => s.Order)
                .FirstOrDefaultAsync();
>>>>>>> origin/feature/-entregable

            if (firstStep != null)
            {
                artifact.CurrentStepId = firstStep.Id;
            }
<<<<<<< HEAD
            var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            firstVersion.FilePath = filePath;
            firstVersion.OriginalFileName = dto.File.FileName;
            firstVersion.ContentType = dto.File.ContentType;
            firstVersion.FileSize = dto.File.Length;  // HU-010
=======
>>>>>>> origin/feature/-entregable
        }

        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();

        // Cargar relaciones para el DTO de respuesta
        await _context.Entry(artifact)
            .Reference(a => a.Workflow)
            .LoadAsync();
        await _context.Entry(artifact)
            .Reference(a => a.CurrentStep)
            .LoadAsync();

        var resultDto = new ArtifactDto
        {
            Id = artifact.Id,
            Name = artifact.Name,
            Description = artifact.Description,
            Type = artifact.Type,
            Status = artifact.Status,
            IsMandatory = artifact.IsMandatory,
            WorkflowId = artifact.WorkflowId,
            WorkflowName = artifact.Workflow?.Name,
            CurrentStepId = artifact.CurrentStepId,
            CurrentStepName = artifact.CurrentStep?.Name,
            CreatedAt = artifact.CreatedAt,
<<<<<<< HEAD
            BuildIdentifier = artifact.BuildIdentifier,
            BuildDownloadUrl = artifact.BuildDownloadUrl,
            ClosureChecklistJson = artifact.ClosureChecklistJson,
            Versions = new List<ArtifactVersionDto> { 
                new ArtifactVersionDto {
                    Id = firstVersion.Id,
                    VersionNumber = firstVersion.VersionNumber,
                    Author = firstVersion.Author,
                    Observations = firstVersion.Observations,  // HU-010
                    Content = firstVersion.Content,
                    OriginalFileName = firstVersion.OriginalFileName,
                    ContentType = firstVersion.ContentType,
                    FileSize = firstVersion.FileSize,
                    CreatedAt = firstVersion.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(firstVersion.FilePath) ? $"/uploads/{Path.GetFileName(firstVersion.FilePath)}" : null
                }
            }
=======
            UpdatedAt = artifact.UpdatedAt
>>>>>>> origin/feature/-entregable
        };

        return CreatedAtAction(nameof(GetArtifact), new { id = artifact.Id }, resultDto);
    }

<<<<<<< HEAD
    // PUT: api/artifacts/{id} - Actualizar artefacto (para checklist, build info, etc.)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtifact(int id, [FromBody] UpdateArtifactDto dto)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        // Actualizar campos generales
        if (dto.Status.HasValue)
        {
            artifact.Status = dto.Status.Value;
        }

        // Actualizar campos de Build Final
        if (!string.IsNullOrEmpty(dto.BuildIdentifier))
        {
            artifact.BuildIdentifier = dto.BuildIdentifier;
        }
        if (!string.IsNullOrEmpty(dto.BuildDownloadUrl))
        {
            artifact.BuildDownloadUrl = dto.BuildDownloadUrl;
        }

        // Actualizar checklist de cierre
        if (!string.IsNullOrEmpty(dto.ClosureChecklistJson))
        {
            artifact.ClosureChecklistJson = dto.ClosureChecklistJson;
        }

        artifact.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ArtifactDto
        {
            Id = artifact.Id,
            Type = artifact.Type,
            ProjectPhaseId = artifact.ProjectPhaseId,
            IsMandatory = artifact.IsMandatory,
            Status = artifact.Status,
            CreatedAt = artifact.CreatedAt,
            BuildIdentifier = artifact.BuildIdentifier,
            BuildDownloadUrl = artifact.BuildDownloadUrl,
            ClosureChecklistJson = artifact.ClosureChecklistJson,
            Versions = artifact.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new ArtifactVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Author = v.Author,
                Observations = v.Observations,  // HU-010
                Content = v.Content,
                OriginalFileName = v.OriginalFileName,
                ContentType = v.ContentType,
                FileSize = v.FileSize,
                CreatedAt = v.CreatedAt,
                DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
            }).ToList()
        });
    }

    // POST: api/artifacts/{id}/versions - Agregar nueva versión
    [HttpPost("{id}/versions")]
    public async Task<IActionResult> AddVersion(int id, [FromForm] AddVersionDto dto)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        var maxVersion = artifact.Versions.Any() ? artifact.Versions.Max(v => v.VersionNumber) : 0;

        var newVersion = new ArtifactVersion
        {
            ArtifactId = artifact.Id,
            VersionNumber = maxVersion + 1,
            Author = dto.Author,
            Observations = dto.Observations,  // HU-010: Descripción de cambios
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

            newVersion.FilePath = filePath;
            newVersion.OriginalFileName = dto.File.FileName;
            newVersion.ContentType = dto.File.ContentType;
            newVersion.FileSize = dto.File.Length;  // HU-010: Tamaño del archivo
        }

        _context.ArtifactVersions.Add(newVersion);
        artifact.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ArtifactVersionDto
        {
            Id = newVersion.Id,
            VersionNumber = newVersion.VersionNumber,
            Author = newVersion.Author,
            Observations = newVersion.Observations,  // HU-010
            Content = newVersion.Content,
            OriginalFileName = newVersion.OriginalFileName,
            ContentType = newVersion.ContentType,
            FileSize = newVersion.FileSize,
            CreatedAt = newVersion.CreatedAt,
            DownloadUrl = !string.IsNullOrEmpty(newVersion.FilePath) ? $"/uploads/{Path.GetFileName(newVersion.FilePath)}" : null
        });
    }

    // GET: api/artifacts/transition/{projectId} - Obtener artefactos de fase Transición
    [HttpGet("transition/{projectId}")]
    public async Task<IActionResult> GetTransitionArtifacts(int projectId)
    {
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Name == "Transición");

        if (transitionPhase == null)
        {
            return NotFound("Transition phase not found for this project.");
        }

        var artifacts = await _context.Artifacts
            .Where(a => a.ProjectPhaseId == transitionPhase.Id)
            .Include(a => a.Versions)
            .Select(a => new ArtifactDto
            {
                Id = a.Id,
                Type = a.Type,
                ProjectPhaseId = a.ProjectPhaseId,
                IsMandatory = a.IsMandatory,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                BuildIdentifier = a.BuildIdentifier,
                BuildDownloadUrl = a.BuildDownloadUrl,
                ClosureChecklistJson = a.ClosureChecklistJson,
                Versions = a.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new ArtifactVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    Author = v.Author,
                    Observations = v.Observations,  // HU-010
                    Content = v.Content,
                    OriginalFileName = v.OriginalFileName,
                    ContentType = v.ContentType,
                    FileSize = v.FileSize,
                    CreatedAt = v.CreatedAt,
                    DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
                }).ToList()
            })
            .ToListAsync();

        // Definir artefactos obligatorios de transición
        var mandatoryTypes = new[] {
            ArtifactType.UserManual,
            ArtifactType.TechnicalManual,
            ArtifactType.DeploymentPlan,
            ArtifactType.ClosureDocument,
            ArtifactType.FinalBuild,
            ArtifactType.BetaTestReport
        };

        var existingTypes = artifacts.Select(a => a.Type).ToHashSet();
        var missingMandatory = mandatoryTypes.Where(t => !existingTypes.Contains(t)).ToList();

        return Ok(new
        {
            phaseId = transitionPhase.Id,
            artifacts,
            mandatoryTypes = mandatoryTypes.Select(t => new { type = t, typeName = t.ToString() }),
            missingMandatory = missingMandatory.Select(t => new { type = t, typeName = t.ToString() }),
            canClose = !missingMandatory.Any() && artifacts.All(a => a.Status == ArtifactStatus.Approved)
        });
    }

    // POST: api/artifacts/validate-closure/{projectId} - Validar cierre del proyecto
    [HttpPost("validate-closure/{projectId}")]
    public async Task<IActionResult> ValidateProjectClosure(int projectId)
    {
        var transitionPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Name == "Transición");

        if (transitionPhase == null)
        {
            return NotFound("Transition phase not found for this project.");
        }

        var artifacts = await _context.Artifacts
            .Where(a => a.ProjectPhaseId == transitionPhase.Id)
            .ToListAsync();

        var mandatoryTypes = new[] {
            ArtifactType.UserManual,
            ArtifactType.TechnicalManual,
            ArtifactType.DeploymentPlan,
            ArtifactType.ClosureDocument,
            ArtifactType.FinalBuild,
            ArtifactType.BetaTestReport
        };

        var existingTypes = artifacts.Select(a => a.Type).ToHashSet();
        var missingMandatory = mandatoryTypes.Where(t => !existingTypes.Contains(t)).ToList();
        var pendingApproval = artifacts.Where(a => a.Status != ArtifactStatus.Approved).ToList();

        // Verificar checklist del documento de cierre
        var closureDoc = artifacts.FirstOrDefault(a => a.Type == ArtifactType.ClosureDocument);
        var checklistValidation = new { isValid = true, pendingItems = new List<string>() };

        if (closureDoc != null && !string.IsNullOrEmpty(closureDoc.ClosureChecklistJson))
        {
            try
            {
                var checklist = JsonSerializer.Deserialize<List<ClosureChecklistItem>>(closureDoc.ClosureChecklistJson, _jsonOptions);
                if (checklist != null)
                {
                    var pendingMandatory = checklist
                        .Where(c => c.IsMandatory && !c.IsCompleted)
                        .Select(c => c.Description)
                        .ToList();

                    checklistValidation = new
                    {
                        isValid = !pendingMandatory.Any(),
                        pendingItems = pendingMandatory
                    };
                }
            }
            catch
            {
                checklistValidation = new { isValid = false, pendingItems = new List<string> { "Error al validar el checklist" } };
            }
        }
        else if (closureDoc == null)
        {
            checklistValidation = new { isValid = false, pendingItems = new List<string> { "Documento de cierre no encontrado" } };
        }

        var canClose = !missingMandatory.Any() 
                      && !pendingApproval.Any() 
                      && checklistValidation.isValid;

        return Ok(new
        {
            canClose,
            missingArtifacts = missingMandatory.Select(t => t.ToString()),
            pendingApproval = pendingApproval.Select(a => new { a.Type, typeName = a.Type.ToString() }),
            checklistValidation
        });
    }

    // HU-010: GET: api/artifacts/{id}/versions - Obtener todas las versiones de un artefacto
    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetVersions(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .Include(a => a.ProjectPhase)
                .ThenInclude(p => p!.Project)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        var versions = artifact.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ArtifactVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Author = v.Author,
                Observations = v.Observations,
                Content = v.Content,
                OriginalFileName = v.OriginalFileName,
                ContentType = v.ContentType,
                FileSize = v.FileSize,
                CreatedAt = v.CreatedAt,
                DownloadUrl = !string.IsNullOrEmpty(v.FilePath) ? $"/uploads/{Path.GetFileName(v.FilePath)}" : null
            })
            .ToList();

        return Ok(new
        {
            artifactId = artifact.Id,
            artifactType = artifact.Type.ToString(),
            projectName = artifact.ProjectPhase?.Project?.Name ?? "Unknown",
            phaseName = artifact.ProjectPhase?.Name ?? "Unknown",
            totalVersions = versions.Count,
            versions
        });
    }

    // HU-010: GET: api/artifacts/{id}/versions/compare - Comparar metadatos de versiones
    [HttpGet("{id}/versions/compare")]
    public async Task<IActionResult> CompareVersions(int id, [FromQuery] int v1, [FromQuery] int v2)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        var version1 = artifact.Versions.FirstOrDefault(v => v.VersionNumber == v1);
        var version2 = artifact.Versions.FirstOrDefault(v => v.VersionNumber == v2);

        if (version1 == null || version2 == null)
        {
            return NotFound("One or both versions not found.");
        }

        var differences = new List<string>();

        // Comparar metadatos
        if (version1.Author != version2.Author)
        {
            differences.Add($"Autor: '{version1.Author}' → '{version2.Author}'");
        }

        if (version1.OriginalFileName != version2.OriginalFileName)
        {
            differences.Add($"Archivo: '{version1.OriginalFileName ?? "N/A"}' → '{version2.OriginalFileName ?? "N/A"}'");
        }

        if (version1.ContentType != version2.ContentType)
        {
            differences.Add($"Tipo de contenido: '{version1.ContentType ?? "N/A"}' → '{version2.ContentType ?? "N/A"}'");
        }

        if (version1.FileSize != version2.FileSize)
        {
            var size1 = version1.FileSize.HasValue ? FormatFileSize(version1.FileSize.Value) : "N/A";
            var size2 = version2.FileSize.HasValue ? FormatFileSize(version2.FileSize.Value) : "N/A";
            differences.Add($"Tamaño: {size1} → {size2}");
        }

        var timeDiff = version2.CreatedAt - version1.CreatedAt;
        differences.Add($"Diferencia de tiempo: {timeDiff.Days} días, {timeDiff.Hours} horas");

        return Ok(new VersionComparisonDto
        {
            Version1 = new ArtifactVersionDto
            {
                Id = version1.Id,
                VersionNumber = version1.VersionNumber,
                Author = version1.Author,
                Observations = version1.Observations,
                Content = version1.Content,
                OriginalFileName = version1.OriginalFileName,
                ContentType = version1.ContentType,
                FileSize = version1.FileSize,
                CreatedAt = version1.CreatedAt,
                DownloadUrl = !string.IsNullOrEmpty(version1.FilePath) ? $"/uploads/{Path.GetFileName(version1.FilePath)}" : null
            },
            Version2 = new ArtifactVersionDto
            {
                Id = version2.Id,
                VersionNumber = version2.VersionNumber,
                Author = version2.Author,
                Observations = version2.Observations,
                Content = version2.Content,
                OriginalFileName = version2.OriginalFileName,
                ContentType = version2.ContentType,
                FileSize = version2.FileSize,
                CreatedAt = version2.CreatedAt,
                DownloadUrl = !string.IsNullOrEmpty(version2.FilePath) ? $"/uploads/{Path.GetFileName(version2.FilePath)}" : null
            },
            Differences = differences
        });
    }

    // HU-010: GET: api/artifacts/{id}/history/export - Exportar historial de versiones
    [HttpGet("{id}/history/export")]
    public async Task<IActionResult> ExportVersionHistory(int id)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .Include(a => a.ProjectPhase)
                .ThenInclude(p => p!.Project)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        var historyExport = new VersionHistoryExportDto
        {
            ArtifactType = artifact.Type.ToString(),
            ProjectName = artifact.ProjectPhase?.Project?.Name ?? "Unknown",
            PhaseName = artifact.ProjectPhase?.Name ?? "Unknown",
            ExportedAt = DateTime.UtcNow,
            Versions = artifact.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new VersionHistoryItemDto
                {
                    VersionNumber = v.VersionNumber,
                    Author = v.Author,
                    Observations = v.Observations,
                    CreatedAt = v.CreatedAt,
                    FileName = v.OriginalFileName,
                    FileSize = v.FileSize
                })
                .ToList()
        };

        // Devolver como JSON descargable
        var json = JsonSerializer.Serialize(historyExport, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        var fileName = $"historial_{artifact.Type}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    // HU-010: GET: api/artifacts/{id}/versions/{versionNumber}/download - Descargar versión específica
    [HttpGet("{id}/versions/{versionNumber}/download")]
    public async Task<IActionResult> DownloadVersion(int id, int versionNumber)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
        {
            return NotFound("Artifact not found.");
        }

        var version = artifact.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber);

        if (version == null)
        {
            return NotFound("Version not found.");
        }

        if (string.IsNullOrEmpty(version.FilePath) || !System.IO.File.Exists(version.FilePath))
        {
            return NotFound("File not found on server.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(version.FilePath);
        var contentType = version.ContentType ?? "application/octet-stream";
        var fileName = version.OriginalFileName ?? $"version_{versionNumber}";

        return File(fileBytes, contentType, fileName);
    }

    // Helper method to format file size
    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}

// DTOs adicionales
public class UpdateArtifactDto
{
    public ArtifactStatus? Status { get; set; }
    public string? BuildIdentifier { get; set; }
    public string? BuildDownloadUrl { get; set; }
    public string? ClosureChecklistJson { get; set; }
}

public class AddVersionDto
{
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    // HU-010: Descripción de cambios obligatoria
    [Required(ErrorMessage = "La descripción de cambios es obligatoria")]
    [MaxLength(2000)]
    public string Observations { get; set; } = string.Empty;

    public string? Content { get; set; }
    public IFormFile? File { get; set; }
}

public class ClosureChecklistItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public string? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
=======
    [HttpPut("{id}/change-step")]
    public async Task<IActionResult> ChangeWorkflowStep(int id, [FromBody] ChangeStepDto dto)
    {
        var artifact = await _context.Artifacts
            .Include(a => a.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(a => a.CurrentStep)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artifact == null)
            return NotFound();

        if (artifact.WorkflowId == null)
            return BadRequest("Este artefacto no tiene un flujo de trabajo asignado");

        var newStep = await _context.WorkflowSteps
            .FirstOrDefaultAsync(s => s.Id == dto.NewStepId && s.WorkflowId == artifact.WorkflowId);

        if (newStep == null)
            return BadRequest("El paso seleccionado no pertenece al flujo de trabajo de este artefacto");

        // Registrar el cambio en el historial
        var history = new ArtifactHistory
        {
            ArtifactId = artifact.Id,
            PreviousState = artifact.CurrentStep?.Name ?? "Sin estado",
            NewState = newStep.Name,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = dto.ChangedBy ?? "Sistema",
            Comments = dto.Comments ?? string.Empty
        };

        artifact.CurrentStepId = dto.NewStepId;
        artifact.UpdatedAt = DateTime.UtcNow;

        _context.ArtifactHistories.Add(history);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<ArtifactHistory>>> GetArtifactHistory(int id)
    {
        var history = await _context.ArtifactHistories
            .Where(h => h.ArtifactId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        return Ok(history);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtifact(int id, CreateArtifactDto dto)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        artifact.Name = dto.Name;
        artifact.Description = dto.Description;
        artifact.Type = dto.Type;
        artifact.IsMandatory = dto.IsMandatory;
        artifact.WorkflowId = dto.WorkflowId;
        artifact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtifact(int id)
    {
        var artifact = await _context.Artifacts.FindAsync(id);
        if (artifact == null)
            return NotFound();

        _context.Artifacts.Remove(artifact);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ChangeStepDto
{
    public int NewStepId { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comments { get; set; }
>>>>>>> origin/feature/-entregable
}