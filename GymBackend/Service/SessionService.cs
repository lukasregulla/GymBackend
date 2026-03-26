using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Session;
using GymBackend.Repository;

namespace GymBackend.Service;

public class SessionService(
    ISessionRepository sessionRepo,
    ITemplateRepository templateRepo,
    IExerciseRepository exerciseRepo) : ISessionService
{
    private static SessionDto ToDto(WorkoutSession s) => new()
    {
        Id = s.Id,
        ScheduledDate = s.ScheduledDate,
        IsCompleted = s.IsCompleted,
        CompletedAt = s.CompletedAt,
        Notes = s.Notes,
        TemplateId = s.TemplateId,
        TemplateName = s.Template?.Name
    };

    private static SessionExerciseDto ToExerciseDto(SessionExercise se) => new()
    {
        SessionExerciseId = se.Id,
        ExerciseId = se.ExerciseId,
        ExerciseName = se.Exercise.Name,
        MuscleGroup = se.Exercise.MuscleGroup,
        OrderIndex = se.OrderIndex
    };

    private static SessionDetailDto ToDetailDto(WorkoutSession s) => new()
    {
        Id = s.Id,
        ScheduledDate = s.ScheduledDate,
        IsCompleted = s.IsCompleted,
        CompletedAt = s.CompletedAt,
        Notes = s.Notes,
        TemplateId = s.TemplateId,
        TemplateName = s.Template?.Name,
        Exercises = s.SessionExercises
            .OrderBy(se => se.OrderIndex)
            .Select(ToExerciseDto)
            .ToList()
    };

    public async Task<List<SessionDto>> GetAllAsync(int userId, DateOnly? from, DateOnly? to) =>
        (await sessionRepo.GetAllByUserAsync(userId, from, to)).Select(ToDto).ToList();

    public async Task<SessionDetailDto> GetByIdAsync(int id, int userId)
    {
        var session = await sessionRepo.GetByIdWithDetailsAsync(id, userId)
            ?? throw new NotFoundException("Session not found.");
        return ToDetailDto(session);
    }

    public async Task<SessionDto> CreateAsync(CreateSessionDto dto, int userId)
    {
        var session = new WorkoutSession
        {
            UserId = userId,
            TemplateId = dto.TemplateId,
            ScheduledDate = dto.ScheduledDate,
            Notes = dto.Notes
        };

        if (dto.TemplateId.HasValue)
        {
            var template = await templateRepo.GetByIdWithExercisesAsync(dto.TemplateId.Value, userId)
                ?? throw new NotFoundException("Template not found.");

            session.SessionExercises = template.TemplateExercises
                .OrderBy(te => te.OrderIndex)
                .Select(te => new SessionExercise
                {
                    ExerciseId = te.ExerciseId,
                    OrderIndex = te.OrderIndex
                })
                .ToList();
        }

        await sessionRepo.CreateAsync(session);

        // Reload with template name for DTO
        var created = await sessionRepo.GetByIdWithDetailsAsync(session.Id, userId)!;
        return ToDetailDto(created!);
    }

    public async Task<SessionDto> CompleteAsync(int id, int userId)
    {
        var session = await sessionRepo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Session not found.");
        session.IsCompleted = true;
        session.CompletedAt = DateTime.UtcNow;
        await sessionRepo.UpdateAsync(session);
        return ToDto(session);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var session = await sessionRepo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Session not found.");
        await sessionRepo.DeleteAsync(session);
    }

    public async Task<SessionExerciseDto> AddExerciseAsync(int sessionId, AddSessionExerciseDto dto, int userId)
    {
        _ = await sessionRepo.GetByIdAsync(sessionId, userId)
            ?? throw new NotFoundException("Session not found.");

        var exercise = await exerciseRepo.GetByIdAsync(dto.ExerciseId, userId)
            ?? throw new NotFoundException("Exercise not found.");

        var se = new SessionExercise
        {
            SessionId = sessionId,
            ExerciseId = dto.ExerciseId,
            OrderIndex = dto.OrderIndex
        };

        await sessionRepo.AddExerciseAsync(se);

        return new SessionExerciseDto
        {
            SessionExerciseId = se.Id,
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            MuscleGroup = exercise.MuscleGroup,
            OrderIndex = se.OrderIndex
        };
    }

    public async Task RemoveExerciseAsync(int sessionId, int sessionExerciseId, int userId)
    {
        var se = await sessionRepo.GetSessionExerciseAsync(sessionId, sessionExerciseId, userId)
            ?? throw new NotFoundException("Exercise not found in session.");
        await sessionRepo.RemoveExerciseAsync(se);
    }
}
