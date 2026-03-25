namespace GymBackend.Model.Dto.Template;

public class ReorderExercisesDto
{
    public List<ReorderItemDto> Items { get; set; } = new();
}

public class ReorderItemDto
{
    public int TemplateExerciseId { get; set; }
    public int OrderIndex { get; set; }
}
