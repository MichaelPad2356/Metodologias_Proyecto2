using backend.Contracts;

namespace backend.Services;

public interface IDefectService
{
    Task<List<DefectDto>> GetAllAsync(int? projectId = null);
    Task<DefectDto?> GetByIdAsync(int id);
    Task<DefectDto> CreateAsync(CreateDefectDto dto);
    Task<DefectDto?> UpdateAsync(int id, UpdateDefectDto dto);
    Task<bool> DeleteAsync(int id);
}
