using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Session;
using GymBackend.Model.Dto.Set;
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
        Name = s.Name,
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
        OrderIndex = se.OrderIndex,
        Sets = se.Sets
            .OrderBy(s => s.SetNumber)
            .Select(s => new SetDto
            {
                Id = s.Id,
                SetNumber = s.SetNumber,
                WeightKg = s.WeightKg,
                Reps = s.Reps,
                IsPersonalBest = s.IsPersonalBest,
                LoggedAt = s.LoggedAt
            })
            .ToList()
    };

    private static SessionDetailDto ToDetailDto(WorkoutSession s) => new()
    {
        Id = s.Id,
        Name = s.Name,
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

    private async Task<List<Set>> CopyLastSetsAsync(int exerciseId, int userId) =>
        (await sessionRepo.GetLastSetsForExerciseAsync(exerciseId, userId))
            .Select(s => new Set
            {
                SetNumber = s.SetNumber,
                WeightKg = s.WeightKg,
                Reps = s.Reps,
                IsPersonalBest = false,
                LoggedAt = DateTime.UtcNow
            }).ToList();

    public async Task<SessionDto> CreateAsync(CreateSessionDto dto, int userId)
    {
        var session = new WorkoutSession
        {
            UserId = userId,
            Name = dto.Name,
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

            foreach (var se in session.SessionExercises)
                se.Sets = await CopyLastSetsAsync(se.ExerciseId, userId);
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
        se.Sets = await CopyLastSetsAsync(dto.ExerciseId, userId);

        await sessionRepo.AddExerciseAsync(se);

        return new SessionExerciseDto
        {
            SessionExerciseId = se.Id,
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            MuscleGroup = exercise.MuscleGroup,
            OrderIndex = se.OrderIndex,
            Sets = se.Sets.OrderBy(s => s.SetNumber).Select(s => new SetDto
            {
                Id = s.Id,
                SetNumber = s.SetNumber,
                WeightKg = s.WeightKg,
                Reps = s.Reps,
                IsPersonalBest = s.IsPersonalBest,
                LoggedAt = s.LoggedAt
            }).ToList()
        };
    }

    public async Task RemoveExerciseAsync(int sessionId, int sessionExerciseId, int userId)
    {
        var se = await sessionRepo.GetSessionExerciseAsync(sessionId, sessionExerciseId, userId)
            ?? throw new NotFoundException("Exercise not found in session.");
        await sessionRepo.RemoveExerciseAsync(se);
    }
}
