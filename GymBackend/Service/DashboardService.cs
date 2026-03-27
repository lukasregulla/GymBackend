using GymBackend.Model;
using GymBackend.Model.Dto.Dashboard;
using GymBackend.Model.Dto.Session;
using GymBackend.Repository;

namespace GymBackend.Service;

public class DashboardService(ISessionRepository sessionRepo) : IDashboardService
{
    private static SessionDto ToDto(WorkoutSession s) => new()
    {
        Id = s.Id,
        ScheduledDate = s.ScheduledDate,
        IsCompleted = s.IsCompleted,
        CompletedAt = s.CompletedAt,
        Notes = s.Notes,
        TemplateId = s.TemplateId,
        TemplateName = s.Template?.Name
    };

    public async Task<WeeklyDashboardDto> GetWeekAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
        var weekStart = today.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(6);

        var sessions = await sessionRepo.GetAllByUserAsync(userId, weekStart, weekEnd);

        return new WeeklyDashboardDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            TotalScheduled = sessions.Count,
            TotalCompleted = sessions.Count(s => s.IsCompleted),
            Sessions = sessions.Select(ToDto).ToList()
        };
    }

    public async Task<List<SessionDto>> GetRecentAsync(int userId, int count)
    {
        var sessions = await sessionRepo.GetRecentCompletedAsync(userId, count);
        return sessions.Select(ToDto).ToList();
    }
}
