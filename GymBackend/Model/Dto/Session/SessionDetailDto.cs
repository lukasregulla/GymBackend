namespace GymBackend.Model.Dto.Session;

public class SessionDetailDto : SessionDto
{
    public List<SessionExerciseDto> Exercises { get; set; } = [];
}
