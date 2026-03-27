using GymBackend.Model.Dto.Dashboard;
using GymBackend.Model.Dto.Session;

namespace GymBackend.Service;

public interface IDashboardService
{
    Task<WeeklyDashboardDto> GetWeekAsync(int userId);
    Task<List<SessionDto>> GetRecentAsync(int userId, int count);
}
