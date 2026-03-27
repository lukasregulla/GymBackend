using GymBackend.Model;

namespace GymBackend.Repository;

public interface ISetRepository
{
    Task<Set?> GetByIdAsync(int setId, int sessionExerciseId,int sessionId, int userId);
    Task<List<Set>> GetAllForExerciseByUserAsync(int exerciseId, int userId);
    public Task<SessionExercise?> GetSessionExerciseAsync(int sessionExeriseId, int sessionId, int userId);
    Task<float?> GetPersonalBestForExerciseByUserAsync(int exerciseId, int userId);
    Task<Set?> GetCurrentPersonalBestSetAsync(int exerciseId, int userId);
    Task<Set> AddAsync(Set set);
    Task UpdateAsync();
    Task DeleteAsync(Set set);
}
