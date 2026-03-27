using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Session;

public class CreateSessionDto
{
    public int? TemplateId { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    [MaxLength(1000)] public string Notes { get; set; } = string.Empty;
}
