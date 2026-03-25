namespace GymBackend.Model
{
    public class TemplateExercise
    {
        public int Id { get; set; }
        public int OrderIndex { get; set; }
        public int DeffaultSets { get; set; }
        public int DeffaultReps { get; set; }

        public int TemplateId { get; set; }
        public WorkoutTemplate Template { get; set; } = null!;

        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

    }
}
