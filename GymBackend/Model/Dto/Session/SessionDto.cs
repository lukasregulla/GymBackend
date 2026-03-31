namespace GymBackend.Model.Dto.Session;

public class SessionDto
{
    public int Id { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? TemplateId { get; set; }
    public string? TemplateName { get; set; }
}
