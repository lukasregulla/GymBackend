using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repository;

public class TemplateRepository(AppDbContext context) : ITemplateRepository
{
    public Task<List<WorkoutTemplate>> GetAllByUserAsync(int userId) =>
        context.WorkoutTemplates.Where(t => t.UserId == userId).ToListAsync();

    public Task<WorkoutTemplate?> GetByIdAsync(int id, int userId) =>
        context.WorkoutTemplates.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public Task<WorkoutTemplate?> GetByIdWithExercisesAsync(int id, int userId) =>
        context.WorkoutTemplates
            .Include(t => t.TemplateExercises)
                .ThenInclude(te => te.Exercise)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<WorkoutTemplate> CreateAsync(WorkoutTemplate template)
    {
        context.WorkoutTemplates.Add(template);
        await context.SaveChangesAsync();
        return template;
    }

    public async Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate template)
    {
        await context.SaveChangesAsync();
        return template;
    }

    public async Task DeleteAsync(WorkoutTemplate template)
    {
        context.WorkoutTemplates.Remove(template);
        await context.SaveChangesAsync();
    }

    public Task<bool> ExerciseAlreadyInTemplateAsync(int templateId, int exerciseId) =>
        context.TemplateExercises
            .AnyAsync(te => te.TemplateId == templateId && te.ExerciseId == exerciseId);

    public async Task<TemplateExercise> AddExerciseAsync(TemplateExercise templateExercise)
    {
        context.TemplateExercises.Add(templateExercise);
        await context.SaveChangesAsync();
        return templateExercise;
    }

    public Task<TemplateExercise?> GetTemplateExerciseAsync(int templateId, int exerciseId, int userId) =>
        context.TemplateExercises
            .FirstOrDefaultAsync(te =>
                te.TemplateId == templateId &&
                te.ExerciseId == exerciseId &&
                te.Template.UserId == userId);

    public async Task RemoveExerciseAsync(TemplateExercise templateExercise)
    {
        context.TemplateExercises.Remove(templateExercise);
        await context.SaveChangesAsync();
    }

    public Task<List<TemplateExercise>> GetTemplateExercisesAsync(int templateId, int userId) =>
        context.TemplateExercises
            .Where(te => te.TemplateId == templateId && te.Template.UserId == userId)
            .ToListAsync();

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
