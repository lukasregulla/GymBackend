using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class CreateRunSessionDto
{
    [MaxLength(100)] public string? Name { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    [MaxLength(1000)] public string Notes { get; set; } = string.Empty;
    [Required][Range(0.01, 1000)] public decimal? DistanceKm { get; set; }
    [Required][Range(1, 86400)] public int? DurationSeconds { get; set; }
    [MaxLength(50)] public string RunType { get; set; } = string.Empty;
}
