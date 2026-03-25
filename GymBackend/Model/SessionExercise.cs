namespace GymBackend.Model
{
    public class SessionExercise
    {
        public int Id { get; set; }
        public int OrderIndex { get; set; }

        public int SessionId { get; set; }
        public WorkoutSession Session { get; set; } = null!;

        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
