namespace GymBackend.Model.Dto.Exercise;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
