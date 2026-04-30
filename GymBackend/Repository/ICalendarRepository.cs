using GymBackend.Model;

namespace GymBackend.Repositories
{
    public interface ICalendarRepository
    {
        Task<List<WorkoutSession>> GetUpcomingIncompleteScheduledSessionsAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByCalendarTokenAsync(string token);
        Task SaveChangesAsync();
    }
}