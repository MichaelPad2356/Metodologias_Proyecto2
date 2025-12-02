using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class DefectService : IDefectService
{
    private readonly ApplicationDbContext _context;

    public DefectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DefectDto>> GetAllAsync(int? projectId = null)
    {
        var query = _context.Defects.AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }

        var defects = await query.ToListAsync();
        return defects.Select(MapToDto).ToList();
    }

    public async Task<DefectDto?> GetByIdAsync(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        return defect == null ? null : MapToDto(defect);
    }

    public async Task<DefectDto> CreateAsync(CreateDefectDto dto)
    {
        var defect = new Defect
        {
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Status = dto.Status,
            ProjectId = dto.ProjectId,
            ArtifactId = dto.ArtifactId,
            ReportedBy = dto.ReportedBy,
            AssignedTo = dto.AssignedTo,
            CreatedAt = DateTime.UtcNow
        };

        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        return MapToDto(defect);
    }

    public async Task<DefectDto?> UpdateAsync(int id, UpdateDefectDto dto)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null) return null;

        if (!string.IsNullOrEmpty(dto.Title)) defect.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description)) defect.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.Severity)) defect.Severity = dto.Severity;
        if (!string.IsNullOrEmpty(dto.Status)) defect.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.AssignedTo)) defect.AssignedTo = dto.AssignedTo;

        await _context.SaveChangesAsync();
        return MapToDto(defect);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null) return false;

        _context.Defects.Remove(defect);
        await _context.SaveChangesAsync();
        return true;
    }

    private static DefectDto MapToDto(Defect d) => new()
    {
        Id = d.Id,
        Title = d.Title,
        Description = d.Description,
        Severity = d.Severity,
        Status = d.Status,
        ProjectId = d.ProjectId,
        ArtifactId = d.ArtifactId,
        ReportedBy = d.ReportedBy,
        AssignedTo = d.AssignedTo,
        CreatedAt = d.CreatedAt
    };
}
