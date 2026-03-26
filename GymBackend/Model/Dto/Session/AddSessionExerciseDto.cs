using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class AddSessionExerciseDto
{
    [Required] public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
}
