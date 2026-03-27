using GymBackend.Model.Dto.Exercise;

namespace GymBackend.Service;

public interface IExerciseService
{
    Task<List<ExerciseDto>> GetAllAsync(int userId);
    Task<ExerciseDto> GetByIdAsync(int id, int userId);
    Task<ExerciseDto> CreateAsync(CreateExerciseDto dto, int userId);
    Task<ExerciseDto> UpdateAsync(int id, UpdateExerciseDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task<ExerciseProgressDto> GetProgressAsync(int exerciseId, int userId);
}
