namespace GymBackend.Model.Dto.Template;

public class TemplateDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public List<TemplateExerciseDto> Exercises { get; set; } = new();
}
