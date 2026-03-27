using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Template;

public class UpdateTemplateDto
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(20)] public string DayOfWeek { get; set; } = string.Empty;
}
