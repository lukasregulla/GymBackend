using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repository;

public class ExerciseRepository(AppDbContext context) : IExerciseRepository
{
    public Task<List<Exercise>> GetAllByUserAsync(int userId) =>
        context.Exercises.Include(e =>e.PersonalBestSet).Where(e => e.UserId == userId).ToListAsync();

    public Task<Exercise?> GetByIdAsync(int id, int userId) =>
        context.Exercises
        .Include(e => e.PersonalBestSet)
        .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task<Exercise> CreateAsync(Exercise exercise)
    {
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task<Exercise> UpdateAsync(Exercise exercise)
    {
        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task DeleteAsync(Exercise exercise)
    {
        context.Exercises.Remove(exercise);
        await context.SaveChangesAsync();
    }

    public Task<bool> IsUsedInSessionAsync(int exerciseId, int userId) =>
        context.SessionExercises
            .AnyAsync(se => se.ExerciseId == exerciseId && se.Session.UserId == userId);
}
