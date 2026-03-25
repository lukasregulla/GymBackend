namespace GymBackend.Model
{
    public class Set
    {
        public int Id { get; set; }
        public int SetNumber { get; set; }
        public float WeightKg { get; set; }
        public int Reps { get; set; }
        public bool IsPersonalBest { get; set; } = false;
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        public int SessionExerciseId { get; set; }
        public SessionExercise SessionExercise { get; set; } = null!;
    }
}
