namespace GymBackend.Services
{
    public interface ICalendarService
    {
        Task<string> GenerateScheduleIcsAsync(int userId);
        Task<string?> GetOrCreateSubscriptionUrlAsync(int userId, string baseUrl);
        Task<string?> ResetSubscriptionUrlAsync(int userId, string baseUrl);
        Task<string?> GenerateScheduleIcsFromTokenAsync(string token);
    }
}