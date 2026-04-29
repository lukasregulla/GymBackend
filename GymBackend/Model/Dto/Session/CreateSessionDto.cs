using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class CreateSessionDto
{
    public int? TemplateId { get; set; }
    [MaxLength(100)] public string? Name { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public TimeOnly? ScheduledStartTime { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    [MaxLength(1000)] public string Notes { get; set; } = string.Empty;
}
