using GymBackend.Model.Dto.Set;

namespace GymBackend.Service;

public interface ISetService
{
    Task<SetDto> LogSetAsync(int sessionId, int sessionExerciseId, LogSetDto dto, int userId);
    Task<SetDto> UpdateSetAsync(int sessionId, int sessionExerciseId, int setId, UpdateSetDto dto, int userId);
    Task DeleteSetAsync(int sessionId, int sessionExerciseId, int setId, int userId);
}
