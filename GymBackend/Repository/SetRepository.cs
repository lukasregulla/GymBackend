using System.ComponentModel;
using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repository;

public class SetRepository(AppDbContext context) : ISetRepository
{
    public Task<Set?> GetByIdAsync(int setId, int sessionExerciseId,int sessionId, int userId) =>
        context.Sets.FirstOrDefaultAsync(s =>
            s.Id == setId &&
            s.SessionExerciseId == sessionExerciseId &&
            s.SessionExercise.SessionId == sessionId &&
            s.SessionExercise.Session.UserId == userId);

    public Task<List<Set>> GetAllForExerciseByUserAsync(int exerciseId, int userId) =>
        context.Sets
            .Include(s => s.SessionExercise)
                .ThenInclude(se => se.Session)
            .Where(s =>
                s.SessionExercise.ExerciseId == exerciseId &&
                s.SessionExercise.Session.UserId == userId)
            .OrderBy(s => s.SessionExercise.Session.ScheduledDate)
            .ToListAsync();

    public Task<SessionExercise?> GetSessionExerciseAsync(int sessionExeriseId, int sessionId, int userId) =>
       context.SessionExercises 
        .Include(se => se.Session)
        .FirstOrDefaultAsync(se =>
            se.Id == sessionExeriseId &&
            se.SessionId == sessionId &&
            se.Session.UserId == userId);

    public async Task<float?> GetPersonalBestForExerciseByUserAsync(int exerciseId, int userId)
    {
        var bestSet = await context.Sets
            .Where(s =>
                s.SessionExercise.ExerciseId == exerciseId &&
                s.SessionExercise.Session.UserId == userId &&
                s.IsPersonalBest)
            .OrderByDescending(s => s.LoggedAt)
            .FirstOrDefaultAsync();
        return bestSet?.WeightKg;
    }

    public async Task<Set?> GetCurrentPersonalBestSetAsync(int exerciseId, int userId) =>
        await context.Sets
            .Where(s =>
                s.SessionExercise.ExerciseId == exerciseId &&
                s.SessionExercise.Session.UserId == userId &&
                s.IsPersonalBest)
            .OrderByDescending(s => s.LoggedAt)
            .FirstOrDefaultAsync();


    public async Task<Set> AddAsync(Set set)
    {
        context.Sets.Add(set);
        await context.SaveChangesAsync();
        return set;
    }

    public Task UpdateAsync() => context.SaveChangesAsync();

    public async Task DeleteAsync(Set set)
    {
        context.Sets.Remove(set);
        await context.SaveChangesAsync();
    }
}
