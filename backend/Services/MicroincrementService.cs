using backend.Contracts;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MicroincrementService : IMicroincrementService
{
    private readonly ApplicationDbContext _context;

    public MicroincrementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MicroincrementDto?> GetByIdAsync(int id)
    {
        var microincrement = await _context.Microincrements.FindAsync(id);
        return microincrement == null ? null : MapToDto(microincrement);
    }

    public async Task<List<MicroincrementDto>> GetAllAsync()
    {
        var microincrements = await _context.Microincrements.ToListAsync();
        return microincrements.Select(MapToDto).ToList();
    }

    public async Task<List<MicroincrementDto>> GetByIterationAsync(int phaseId)
    {
        var microincrements = await _context.Microincrements
            .Where(m => m.ProjectPhaseId == phaseId)
            .ToListAsync();
        return microincrements.Select(MapToDto).ToList();
    }

    public async Task<List<MicroincrementDto>> GetByDeliverableAsync(int deliverableId)
    {
        var microincrements = await _context.Microincrements
            .Where(m => m.DeliverableId == deliverableId)
            .ToListAsync();
        return microincrements.Select(MapToDto).ToList();
    }

    public async Task<List<MicroincrementDto>> GetByAuthorAsync(string author)
    {
        var microincrements = await _context.Microincrements
            .Where(m => m.Author == author)
            .ToListAsync();
        return microincrements.Select(MapToDto).ToList();
    }

    public async Task<MicroincrementDto> CreateAsync(CreateMicroincrementDto dto)
    {
        var microincrement = new Microincrement
        {
            Title = dto.Title,
            Description = dto.Description,
            ProjectPhaseId = dto.ProjectPhaseId,
            DeliverableId = dto.DeliverableId,
            Author = dto.Author,
            Date = DateTime.UtcNow
        };

        _context.Microincrements.Add(microincrement);
        await _context.SaveChangesAsync();

        return MapToDto(microincrement);
    }

    public async Task<MicroincrementDto?> UpdateAsync(int id, UpdateMicroincrementDto dto)
    {
        var microincrement = await _context.Microincrements.FindAsync(id);
        if (microincrement == null) return null;

        if (!string.IsNullOrEmpty(dto.Title))
            microincrement.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description))
            microincrement.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.Author))
            microincrement.Author = dto.Author;

        microincrement.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(microincrement);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var microincrement = await _context.Microincrements.FindAsync(id);
        if (microincrement == null) return false;

        _context.Microincrements.Remove(microincrement);
        await _context.SaveChangesAsync();
        return true;
    }

    private static MicroincrementDto MapToDto(Microincrement m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Description = m.Description,
        Date = m.Date,
        Author = m.Author,
        ProjectPhaseId = m.ProjectPhaseId,
        DeliverableId = m.DeliverableId
    };
}