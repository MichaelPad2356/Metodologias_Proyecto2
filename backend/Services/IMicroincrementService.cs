using backend.Contracts;

namespace backend.Services;

public interface IMicroincrementService
{
    Task<MicroincrementDto?> GetByIdAsync(int id);
    Task<List<MicroincrementDto>> GetAllAsync();
    Task<List<MicroincrementDto>> GetByIterationAsync(int phaseId);
    Task<List<MicroincrementDto>> GetByDeliverableAsync(int deliverableId);
    Task<List<MicroincrementDto>> GetByAuthorAsync(string author);
    Task<MicroincrementDto> CreateAsync(CreateMicroincrementDto dto);
    Task<MicroincrementDto?> UpdateAsync(int id, UpdateMicroincrementDto dto);
    Task<bool> DeleteAsync(int id);
}