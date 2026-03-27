using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Template;

public class ReorderExercisesDto
{
    [Required] public List<ReorderItemDto> Items { get; set; } = new();
}

public class ReorderItemDto
{
    [Required] public int TemplateExerciseId { get; set; }
    [Range(0, 1000)] public int OrderIndex { get; set; }
}
