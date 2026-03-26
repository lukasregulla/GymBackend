namespace GymBackend.Model.Dto.Session;

public class SessionExerciseDto
{
    public int SessionExerciseId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public int OrderIndex { get; set; }
}
