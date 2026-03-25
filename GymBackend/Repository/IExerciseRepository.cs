using GymBackend.Model;

namespace GymBackend.Repository;

public interface IExerciseRepository
{
    Task<List<Exercise>> GetAllByUserAsync(int userId);
    Task<Exercise?> GetByIdAsync(int id, int userId);
    Task<Exercise> CreateAsync(Exercise exercise);
    Task<Exercise> UpdateAsync(Exercise exercise);
    Task DeleteAsync(Exercise exercise);
    Task<bool> IsUsedInSessionAsync(int exerciseId, int userId);
}
