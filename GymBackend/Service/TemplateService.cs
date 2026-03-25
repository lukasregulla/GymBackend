using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Template;
using GymBackend.Repository;

namespace GymBackend.Service;

public class TemplateService(ITemplateRepository templateRepo, IExerciseRepository exerciseRepo) : ITemplateService
{
    private static TemplateDto ToDto(WorkoutTemplate t) =>
        new() { Id = t.Id, Name = t.Name, Description = t.Description, DayOfWeek = t.DayOfWeek };

    private static TemplateExerciseDto ToExerciseDto(TemplateExercise te) =>
        new()
        {
            TemplateExerciseId = te.Id,
            ExerciseId = te.ExerciseId,
            ExerciseName = te.Exercise.Name,
            MuscleGroup = te.Exercise.MuscleGroup,
            OrderIndex = te.OrderIndex,
            DefaultSets = te.DeffaultSets,
            DefaultReps = te.DeffaultReps
        };

    private static TemplateDetailDto ToDetailDto(WorkoutTemplate t) =>
        new()
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DayOfWeek = t.DayOfWeek,
            Exercises = t.TemplateExercises
                .OrderBy(te => te.OrderIndex)
                .Select(ToExerciseDto)
                .ToList()
        };

    public async Task<List<TemplateDto>> GetAllAsync(int userId) =>
        (await templateRepo.GetAllByUserAsync(userId)).Select(ToDto).ToList();

    public async Task<TemplateDetailDto> GetByIdAsync(int id, int userId)
    {
        var template = await templateRepo.GetByIdWithExercisesAsync(id, userId)
            ?? throw new NotFoundException("Template not found.");
        return ToDetailDto(template);
    }

    public async Task<TemplateDto> CreateAsync(CreateTemplateDto dto, int userId)
    {
        var template = new WorkoutTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            DayOfWeek = dto.DayOfWeek,
            UserId = userId
        };
        return ToDto(await templateRepo.CreateAsync(template));
    }

    public async Task<TemplateDto> UpdateAsync(int id, UpdateTemplateDto dto, int userId)
    {
        var template = await templateRepo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Template not found.");
        template.Name = dto.Name;
        template.Description = dto.Description;
        template.DayOfWeek = dto.DayOfWeek;
        return ToDto(await templateRepo.UpdateAsync(template));
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var template = await templateRepo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Template not found.");
        await templateRepo.DeleteAsync(template);
    }

    public async Task<TemplateExerciseDto> AddExerciseAsync(int templateId, AddTemplateExerciseDto dto, int userId)
    {
        _ = await templateRepo.GetByIdAsync(templateId, userId)
            ?? throw new NotFoundException("Template not found.");

        var exercise = await exerciseRepo.GetByIdAsync(dto.ExerciseId, userId)
            ?? throw new NotFoundException("Exercise not found.");

        if (await templateRepo.ExerciseAlreadyInTemplateAsync(templateId, dto.ExerciseId))
            throw new BadRequestException("Exercise is already in this template.");

        var te = new TemplateExercise
        {
            TemplateId = templateId,
            ExerciseId = dto.ExerciseId,
            OrderIndex = dto.OrderIndex,
            DeffaultSets = dto.DefaultSets,
            DeffaultReps = dto.DefaultReps
        };

        await templateRepo.AddExerciseAsync(te);

        return new TemplateExerciseDto
        {
            TemplateExerciseId = te.Id,
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            MuscleGroup = exercise.MuscleGroup,
            OrderIndex = te.OrderIndex,
            DefaultSets = te.DeffaultSets,
            DefaultReps = te.DeffaultReps
        };
    }

    public async Task RemoveExerciseAsync(int templateId, int exerciseId, int userId)
    {
        var te = await templateRepo.GetTemplateExerciseAsync(templateId, exerciseId, userId)
            ?? throw new NotFoundException("Exercise not found in template.");
        await templateRepo.RemoveExerciseAsync(te);
    }

    public async Task ReorderExercisesAsync(int templateId, ReorderExercisesDto dto, int userId)
    {
        _ = await templateRepo.GetByIdAsync(templateId, userId)
            ?? throw new NotFoundException("Template not found.");

        var rows = await templateRepo.GetTemplateExercisesAsync(templateId, userId);
        var lookup = rows.ToDictionary(te => te.Id);

        foreach (var item in dto.Items)
        {
            if (lookup.TryGetValue(item.TemplateExerciseId, out var te))
                te.OrderIndex = item.OrderIndex;
        }

        await templateRepo.SaveChangesAsync();
    }
}
