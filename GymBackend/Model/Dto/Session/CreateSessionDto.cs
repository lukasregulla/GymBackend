namespace GymBackend.Model.Dto.Session;

public class CreateSessionDto
{
    public int? TemplateId { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
