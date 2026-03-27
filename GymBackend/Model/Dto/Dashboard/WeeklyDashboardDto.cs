using GymBackend.Model.Dto.Session;

namespace GymBackend.Model.Dto.Dashboard;

public class WeeklyDashboardDto
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int TotalScheduled { get; set; }
    public int TotalCompleted { get; set; }
    public List<SessionDto> Sessions { get; set; } = new();
}
