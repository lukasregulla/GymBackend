namespace GymBackend.Model.Dto.Session;

public class RunSessionDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int DurationSeconds { get; set; }
    public int AveragePaceSecondsPerKm { get; set; }
    public string RunType { get; set; } = string.Empty;
}
