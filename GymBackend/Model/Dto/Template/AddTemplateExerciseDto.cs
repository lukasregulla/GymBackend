using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Template;

public class AddTemplateExerciseDto
{
    [Required] public int ExerciseId { get; set; }
    public int DefaultSets { get; set; } = 3;
    public int DefaultReps { get; set; } = 10;
    public int OrderIndex { get; set; }
}
