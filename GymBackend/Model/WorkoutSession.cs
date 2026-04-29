namespace GymBackend.Model
{
    public class WorkoutSession
    {
        public int Id { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public TimeOnly? ScheduledStartTime { get; set; }
        public int? EstimatedDurationMinutes { get; set; } = 60;
        public DateTime? CompletedAt { get; set; }
        public string? Name { get; set; }
        public string Notes { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? TemplateId { get; set; }
        public WorkoutTemplate? Template { get; set; }

        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();

        public string SessionType { get; set; } = "Strength";
        public RunDetail? RunDetail { get; set; }
    }
}
