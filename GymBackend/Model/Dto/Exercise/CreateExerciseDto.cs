using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Exercise;

public class CreateExerciseDto
{
    [Required] [StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(100)] public string MuscleGroup { get; set; } = string.Empty;
    [StringLength(500)] public string Notes { get; set; } = string.Empty;
}
