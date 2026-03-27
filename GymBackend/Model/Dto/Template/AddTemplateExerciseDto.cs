using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Template;

public class AddTemplateExerciseDto
{
    [Required] public int ExerciseId { get; set; }
    [Range(1, 100)] public int DefaultSets { get; set; } = 3;
    [Range(1, 100)] public int DefaultReps { get; set; } = 10;
    [Range(0, 1000)] public int OrderIndex { get; set; }
}
