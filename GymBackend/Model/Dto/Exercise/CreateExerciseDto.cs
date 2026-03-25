using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Exercise;

public class CreateExerciseDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string MuscleGroup { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
