namespace GymBackend.Model.Dto.Template;

public class TemplateExerciseDto
{
    public int TemplateExerciseId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DefaultSets { get; set; }
    public int DefaultReps { get; set; }
}
