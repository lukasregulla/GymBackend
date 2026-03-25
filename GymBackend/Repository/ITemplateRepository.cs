using GymBackend.Model;

namespace GymBackend.Repository;

public interface ITemplateRepository
{
    Task<List<WorkoutTemplate>> GetAllByUserAsync(int userId);
    Task<WorkoutTemplate?> GetByIdAsync(int id, int userId);
    Task<WorkoutTemplate?> GetByIdWithExercisesAsync(int id, int userId);
    Task<WorkoutTemplate> CreateAsync(WorkoutTemplate template);
    Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate template);
    Task DeleteAsync(WorkoutTemplate template);
    Task<bool> ExerciseAlreadyInTemplateAsync(int templateId, int exerciseId);
    Task<TemplateExercise> AddExerciseAsync(TemplateExercise templateExercise);
    Task<TemplateExercise?> GetTemplateExerciseAsync(int templateId, int exerciseId, int userId);
    Task RemoveExerciseAsync(TemplateExercise templateExercise);
    Task<List<TemplateExercise>> GetTemplateExercisesAsync(int templateId, int userId);
    Task SaveChangesAsync();
}
