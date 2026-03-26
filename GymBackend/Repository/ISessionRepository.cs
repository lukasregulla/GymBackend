using GymBackend.Model;

namespace GymBackend.Repository;

public interface ISessionRepository
{
    Task<List<WorkoutSession>> GetAllByUserAsync(int userId, DateOnly? from, DateOnly? to);
    Task<WorkoutSession?> GetByIdAsync(int id, int userId);
    Task<WorkoutSession?> GetByIdWithDetailsAsync(int id, int userId);
    Task<WorkoutSession> CreateAsync(WorkoutSession session);
    Task<WorkoutSession> UpdateAsync(WorkoutSession session);
    Task DeleteAsync(WorkoutSession session);
    Task<SessionExercise?> GetSessionExerciseAsync(int sessionId, int sessionExerciseId, int userId);
    Task AddExerciseAsync(SessionExercise se);
    Task RemoveExerciseAsync(SessionExercise se);
    Task SaveChangesAsync();
}
