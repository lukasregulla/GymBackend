using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Session;
using GymBackend.Repository;

namespace GymBackend.Service;

public class RunSessionService(IRunSessionRepository repo) : IRunSessionService
{
    private static RunSessionDto ToDto(WorkoutSession s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        ScheduledDate = s.ScheduledDate,
        CompletedAt = s.CompletedAt,
        IsCompleted = s.IsCompleted,
        Notes = s.Notes,
        DistanceKm = s.RunDetail!.DistanceKm,
        DurationSeconds = s.RunDetail.DurationSeconds,
        AveragePaceSecondsPerKm = s.RunDetail.AveragePaceSecondsPerKm,
        RunType = s.RunDetail.RunType
    };

    public async Task<RunSessionDto> CreateAsync(CreateRunSessionDto dto, int userId)
    {
        if (dto.DistanceKm <= 0)
            throw new BadRequestException("DistanceKm must be greater than 0.");
        if (dto.DurationSeconds <= 0)
            throw new BadRequestException("DurationSeconds must be greater than 0.");

        var averagePace = (int)(dto.DurationSeconds / (double)dto.DistanceKm);

        var session = new WorkoutSession
        {
            UserId = userId,
            Name = dto.Name,
            ScheduledDate = dto.ScheduledDate,
            Notes = dto.Notes,
            SessionType = "Run",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow,
            RunDetail = new RunDetail
            {
                DistanceKm = dto.DistanceKm,
                DurationSeconds = dto.DurationSeconds,
                AveragePaceSecondsPerKm = averagePace,
                RunType = dto.RunType
            }
        };

        await repo.CreateAsync(session);

        var created = await repo.GetByIdAsync(session.Id, userId);
        return ToDto(created!);
    }

    public async Task<RunSessionDto> ScheduleAsync(ScheduleRunSessionDto dto, int userId)
    {
        var session = new WorkoutSession
        {
            UserId = userId,
            Name = dto.Name,
            ScheduledDate = dto.ScheduledDate,
            Notes = dto.Notes,
            SessionType = "Run",
            IsCompleted = false,
            CompletedAt = null,
            RunDetail = new RunDetail
            {
                DistanceKm = dto.DistanceKm,
                DurationSeconds = null,
                AveragePaceSecondsPerKm = null,
                RunType = dto.RunType ?? string.Empty
            }
        };

        await repo.CreateAsync(session);

        var created = await repo.GetByIdAsync(session.Id, userId);
        return ToDto(created!);
    }

    public async Task<RunSessionDto> CompleteAsync(int id, CompleteScheduledRunDto dto, int userId)
    {
        var session = await repo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Run session not found.");

        if (session.IsCompleted)
            throw new BadRequestException("Run session is already completed.");

        session.IsCompleted = true;
        session.CompletedAt = DateTime.UtcNow;

        if (dto.Notes is not null)
            session.Notes = dto.Notes;

        session.RunDetail!.DistanceKm = dto.DistanceKm;
        session.RunDetail.DurationSeconds = dto.DurationSeconds;
        session.RunDetail.AveragePaceSecondsPerKm = (int)(dto.DurationSeconds / (double)dto.DistanceKm);

        if (dto.RunType is not null)
            session.RunDetail.RunType = dto.RunType;

        await repo.UpdateAsync(session);

        var updated = await repo.GetByIdAsync(id, userId);
        return ToDto(updated!);
    }

    public async Task<List<RunSessionDto>> GetAllAsync(int userId) =>
        (await repo.GetAllByUserAsync(userId)).Select(ToDto).ToList();

    public async Task<RunSessionDto> GetByIdAsync(int id, int userId)
    {
        var session = await repo.GetByIdAsync(id, userId)
            ?? throw new NotFoundException("Run session not found.");
        return ToDto(session);
    }
}
