namespace GymBackend.Model;

public class RunDetail
{
    public int Id { get; set; }
    public int WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;
    public decimal? DistanceKm { get; set; }
    public int? DurationSeconds { get; set; }
    public int? AveragePaceSecondsPerKm { get; set; }
    public string RunType { get; set; } = string.Empty;
}
