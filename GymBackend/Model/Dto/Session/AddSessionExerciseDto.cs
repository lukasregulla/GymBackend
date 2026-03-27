using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class AddSessionExerciseDto
{
    [Required] public int ExerciseId { get; set; }
    [Range(0, 1000)] public int OrderIndex { get; set; }
}
