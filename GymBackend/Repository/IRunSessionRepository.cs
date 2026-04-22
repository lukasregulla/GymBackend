using GymBackend.Model;

namespace GymBackend.Repository;

public interface IRunSessionRepository
{
    Task<WorkoutSession> CreateAsync(WorkoutSession session);
    Task<List<WorkoutSession>> GetAllByUserAsync(int userId);
    Task<WorkoutSession?> GetByIdAsync(int id, int userId);
}
