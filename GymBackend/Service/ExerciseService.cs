using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Exercise;
using GymBackend.Repository;

namespace GymBackend.Service;

public class ExerciseService(IExerciseRepository repo) : IExerciseService
{
    private static ExerciseDto ToDto(Exercise e) =>
        new() { Id = e.Id, Name = e.Name, MuscleGroup = e.MuscleGroup, Notes = e.Notes };

    public async Task<List<ExerciseDto>> GetAllAsync(int userId) =>
        (await repo.GetAllByUserAsync(userId)).Select(ToDto).ToList();

    public async Task<ExerciseDto> GetByIdAsync(int id, int userId)
    {
        var exercise = await repo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Exercise not found.");
        return ToDto(exercise);
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseDto dto, int userId)
    {
        var exercise = new Exercise
        {
            Name = dto.Name,
            MuscleGroup = dto.MuscleGroup,
            Notes = dto.Notes,
            UserId = userId
        };
        return ToDto(await repo.CreateAsync(exercise));
    }

    public async Task<ExerciseDto> UpdateAsync(int id, UpdateExerciseDto dto, int userId)
    {
        var exercise = await repo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Exercise not found.");
        exercise.Name = dto.Name;
        exercise.MuscleGroup = dto.MuscleGroup;
        exercise.Notes = dto.Notes;
        return ToDto(await repo.UpdateAsync(exercise));
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var exercise = await repo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Exercise not found.");
        if (await repo.IsUsedInSessionAsync(id, userId))
            throw new BadRequestException("Exercise is used in one or more workout sessions and cannot be deleted.");
        await repo.DeleteAsync(exercise);
    }
}
