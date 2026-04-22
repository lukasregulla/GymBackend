using GymBackend.Model.Dto.Session;

namespace GymBackend.Service;

public interface IRunSessionService
{
    Task<RunSessionDto> CreateAsync(CreateRunSessionDto dto, int userId);
    Task<List<RunSessionDto>> GetAllAsync(int userId);
    Task<RunSessionDto> GetByIdAsync(int id, int userId);
}
