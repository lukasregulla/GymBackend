namespace GymBackend.Model.Dto.Exercise;

public class ExerciseProgressDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public float PersonalBest { get; set; }
    public List<ProgressPointDto> History { get; set; } = new();
}
