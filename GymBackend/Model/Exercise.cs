namespace GymBackend.Model
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? PersonalBestSetId { get; set; }
        public Set? PersonalBestSet { get; set; }

        public ICollection<TemplateExercise> TemplateExercises { get; set; } = new List<TemplateExercise>();
        public ICollection<SessionExercise> SessionExercises { get; set; } = new List<SessionExercise>();
    }
}
