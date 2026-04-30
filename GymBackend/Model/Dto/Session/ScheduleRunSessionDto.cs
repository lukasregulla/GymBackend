using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class ScheduleRunSessionDto
{
    [MaxLength(100)] public string? Name { get; set; }
    [Required] public DateOnly ScheduledDate { get; set; }
    public TimeOnly? ScheduledStartTime { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    [MaxLength(1000)] public string Notes { get; set; } = string.Empty;
    [Range(0.01, 1000)] public decimal? DistanceKm { get; set; }
    [MaxLength(50)] public string? RunType { get; set; }
}
