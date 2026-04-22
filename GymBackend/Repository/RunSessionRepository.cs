using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repository;

public class RunSessionRepository(AppDbContext context) : IRunSessionRepository
{
    public async Task<WorkoutSession> CreateAsync(WorkoutSession session)
    {
        context.WorkoutSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<WorkoutSession> UpdateAsync(WorkoutSession session)
    {
        context.WorkoutSessions.Update(session);
        await context.SaveChangesAsync();
        return session;
    }

    public Task<List<WorkoutSession>> GetAllByUserAsync(int userId) =>
        context.WorkoutSessions
            .Include(s => s.RunDetail)
            .Where(s => s.UserId == userId && s.SessionType == "Run")
            .OrderByDescending(s => s.ScheduledDate)
            .ToListAsync();

    public Task<WorkoutSession?> GetByIdAsync(int id, int userId) =>
        context.WorkoutSessions
            .Include(s => s.RunDetail)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.SessionType == "Run");
}
