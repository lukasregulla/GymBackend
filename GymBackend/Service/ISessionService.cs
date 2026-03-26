using GymBackend.Model.Dto.Session;

namespace GymBackend.Service;

public interface ISessionService
{
    Task<List<SessionDto>> GetAllAsync(int userId, DateOnly? from, DateOnly? to);
    Task<SessionDetailDto> GetByIdAsync(int id, int userId);
    Task<SessionDto> CreateAsync(CreateSessionDto dto, int userId);
    Task<SessionDto> CompleteAsync(int id, int userId);
    Task DeleteAsync(int id, int userId);
    Task<SessionExerciseDto> AddExerciseAsync(int sessionId, AddSessionExerciseDto dto, int userId);
    Task RemoveExerciseAsync(int sessionId, int sessionExerciseId, int userId);
}
