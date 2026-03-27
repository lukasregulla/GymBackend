using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Set;

public class LogSetDto
{
    [Required] public int SetNumber { get; set; }
    [Range(0, float.MaxValue)] public float WeightKg { get; set; }
    [Range(1, int.MaxValue)] public int Reps { get; set; }
}
