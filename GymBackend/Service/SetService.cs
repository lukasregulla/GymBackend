using GymBackend.Exceptions;
using GymBackend.Model;
using GymBackend.Model.Dto.Set;
using GymBackend.Repository;

namespace GymBackend.Service;

public class SetService(
    ISetRepository setRepo,
    ISessionRepository sessionRepo,
    IExerciseRepository exerciseRepo) : ISetService
{
    private static SetDto ToDto(Set s) => new()
    {
        Id = s.Id,
        SetNumber = s.SetNumber,
        WeightKg = s.WeightKg,
        Reps = s.Reps,
        IsPersonalBest = s.IsPersonalBest,
        LoggedAt = s.LoggedAt
    };

    private async Task UpdateExercisePersonalBest(int exerciseId, int userId)
    {
        var exercise = await exerciseRepo.GetByIdAsync(exerciseId, userId);
        if (exercise == null) return;

        var bestSet = await setRepo.GetCurrentPersonalBestSetAsync(exerciseId, userId);
        exercise.PersonalBestSetId = bestSet?.Id;
        await exerciseRepo.UpdateAsync(exercise);

    }

    private static void RecalculatePersonalBests(List<Set> allSets)
    {
        if (allSets.Count == 0) return;
        float maxWeight = allSets.Max(s => s.WeightKg);
        var newBest = allSets
            .Where(s => s.WeightKg == maxWeight)
            .OrderBy(s => s.LoggedAt)
            .First();
        foreach (var s in allSets)
            s.IsPersonalBest = s.Id == newBest.Id;
    }


    public async Task<SetDto> LogSetAsync(int sessionId, int sessionExerciseId, LogSetDto dto, int userId)
    {
        var se = await sessionRepo.GetSessionExerciseAsync(sessionId, sessionExerciseId, userId)
            ?? throw new NotFoundException("Session exercise not found.");

        var allExistingSets = await setRepo.GetAllForExerciseByUserAsync(se.ExerciseId, userId);

        var set = new Set
        {
            SessionExerciseId = sessionExerciseId,
            SetNumber = dto.SetNumber,
            WeightKg = dto.WeightKg,
            Reps = dto.Reps,
            IsPersonalBest = false
        };

        float currentBest = allExistingSets.Count > 0 ? allExistingSets.Max(s => s.WeightKg) : 0f;

        if (dto.WeightKg > currentBest)
        {
            set.IsPersonalBest = true;
            var previousBest = allExistingSets.FirstOrDefault(s => s.IsPersonalBest);
            if (previousBest != null)
                previousBest.IsPersonalBest = false;
        }

        await setRepo.AddAsync(set);
        await UpdateExercisePersonalBest(se.ExerciseId, userId);
        return ToDto(set);
    }

    public async Task<SetDto> UpdateSetAsync(int sessionId, int sessionExerciseId, int setId, UpdateSetDto dto, int userId)
    {
        var se = await sessionRepo.GetSessionExerciseAsync(sessionId, sessionExerciseId, userId)
            ?? throw new NotFoundException("Session exercise not found.");

        var set = await setRepo.GetByIdAsync(setId, sessionExerciseId, sessionId, userId)
            ?? throw new NotFoundException("Set not found.");

        set.SetNumber = dto.SetNumber;
        set.WeightKg = dto.WeightKg;
        set.Reps = dto.Reps;

        var allSets = await setRepo.GetAllForExerciseByUserAsync(se.ExerciseId, userId);
        RecalculatePersonalBests(allSets);
        await setRepo.UpdateAsync();
        await UpdateExercisePersonalBest(se.ExerciseId, userId);

        return ToDto(set);
    }

    public async Task DeleteSetAsync(int sessionId, int sessionExerciseId, int setId, int userId)
    {
        var se = await sessionRepo.GetSessionExerciseAsync(sessionId, sessionExerciseId, userId)
            ?? throw new NotFoundException("Session exercise not found.");

        var set = await setRepo.GetByIdAsync(setId, sessionExerciseId, sessionId, userId)
            ?? throw new NotFoundException("Set not found.");

        bool wasPersonalBest = set.IsPersonalBest;
        await setRepo.DeleteAsync(set);

        if (wasPersonalBest)
        {
            var remaining = await setRepo.GetAllForExerciseByUserAsync(se.ExerciseId, userId);
            if (remaining.Count > 0)
            {
                RecalculatePersonalBests(remaining);
                await setRepo.UpdateAsync();
            }
        }
        await UpdateExercisePersonalBest(se.ExerciseId, userId);
    }
}
