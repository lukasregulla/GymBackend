using GymBackend.Data;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Repositories
{
    public class CalendarRepository : ICalendarRepository
    {
        private readonly AppDbContext _context;

        public CalendarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkoutSession>> GetUpcomingIncompleteScheduledSessionsAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var maxDate = today.AddDays(90);

            return await _context.WorkoutSessions
                .Where(s =>
                    s.UserId == userId &&
                    !s.IsCompleted &&
                    s.ScheduledDate != null &&
                    s.ScheduledStartTime != null &&
                    s.ScheduledDate >= today &&
                    s.ScheduledDate <= maxDate)
                .Include(s => s.Template)
                .Include(s => s.SessionExercises)
                    .ThenInclude(se => se.Exercise)
                .Include(s => s.RunDetail)
                .OrderBy(s => s.ScheduledDate)
                .ThenBy(s => s.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByCalendarTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.CalendarSubscriptionToken == token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}