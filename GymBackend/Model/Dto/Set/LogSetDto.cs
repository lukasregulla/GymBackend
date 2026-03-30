using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Set;

public class LogSetDto
{
    [Required] [Range(1, 100)] public int SetNumber { get; set; }
    [Range(0.5, 1000)] public float WeightKg { get; set; }
    [Range(1, 200)] public int Reps { get; set; }
}
