using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repository;

public class SessionRepository(AppDbContext context) : ISessionRepository
{
    public Task<List<WorkoutSession>> GetAllByUserAsync(int userId, DateOnly? from, DateOnly? to)
    {
        var query = context.WorkoutSessions
            .Include(s => s.Template)
            .Where(s => s.UserId == userId);

        if (from.HasValue)
            query = query.Where(s => s.ScheduledDate >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.ScheduledDate <= to.Value);

        return query
            .OrderBy(s => s.ScheduledDate)
            .ThenBy(s => s.ScheduledStartTime)
            .ToListAsync();
    }

    public Task<WorkoutSession?> GetByIdAsync(int id, int userId) =>
        context.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

    public Task<WorkoutSession?> GetByIdWithDetailsAsync(int id, int userId) =>
        context.WorkoutSessions
            .Include(s => s.Template)
            .Include(s => s.SessionExercises)
                .ThenInclude(se => se.Exercise)
            .Include(s => s.SessionExercises)
                .ThenInclude(se => se.Sets)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

    public async Task<WorkoutSession> CreateAsync(WorkoutSession session)
    {
        context.WorkoutSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<WorkoutSession> UpdateAsync(WorkoutSession session)
    {
        await context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteAsync(WorkoutSession session)
    {
        context.WorkoutSessions.Remove(session);
        await context.SaveChangesAsync();
    }

    public Task<SessionExercise?> GetSessionExerciseAsync(int sessionId, int sessionExerciseId, int userId) =>
        context.SessionExercises
            .FirstOrDefaultAsync(se =>
                se.Id == sessionExerciseId &&
                se.SessionId == sessionId &&
                se.Session.UserId == userId);

    public async Task AddExerciseAsync(SessionExercise se)
    {
        context.SessionExercises.Add(se);
        await context.SaveChangesAsync();
    }

    public async Task RemoveExerciseAsync(SessionExercise se)
    {
        context.SessionExercises.Remove(se);
        await context.SaveChangesAsync();
    }

    public Task<List<WorkoutSession>> GetRecentCompletedAsync(int userId, int count) =>
        context.WorkoutSessions
            .Include(s => s.Template)
            .Where(s => s.UserId == userId && s.IsCompleted)
            .OrderByDescending(s => s.CompletedAt)
            .Take(count)
            .ToListAsync();

    public async Task<List<Set>> GetLastSetsForExerciseAsync(int exerciseId, int userId)
    {
        var lastSe = await context.SessionExercises
            .Include(se => se.Sets)
            .Where(se =>
                se.ExerciseId == exerciseId &&
                se.Session.UserId == userId &&
                se.Session.IsCompleted &&
                se.Sets.Any())
            .OrderByDescending(se => se.Session.CompletedAt)
            .FirstOrDefaultAsync();

        return lastSe?.Sets.OrderBy(s => s.SetNumber).ToList() ?? [];
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
