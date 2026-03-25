using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Template;

public class UpdateTemplateDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
}
