using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GymBackend.Data;
using GymBackend.Model;

namespace GymBackend.Service
{
    public class SeedDataService : ISeedDataService
    {
        private readonly AppDbContext _context;

        private static SeedFile? _seedFile;
        private static readonly Lock _lock = new();

        public SeedDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedForUserAsync(int userId)
        {
            var seed = LoadSeedFile();

            var exercises = seed.Exercises.Select(e => new Exercise
            {
                Name = e.Name,
                MuscleGroup = e.MuscleGroup,
                Notes = e.Notes,
                UserId = userId
            }).ToList();

            await _context.Exercises.AddRangeAsync(exercises);
            await _context.SaveChangesAsync();

            var nameToId = exercises.ToDictionary(e => e.Name, e => e.Id);

            foreach (var t in seed.Templates)
            {
                var template = new WorkoutTemplate
                {
                    Name = t.Name,
                    Description = t.Description,
                    DayOfWeek = t.DayOfWeek,
                    UserId = userId
                };
                await _context.WorkoutTemplates.AddAsync(template);
                await _context.SaveChangesAsync();

                var templateExercises = t.Exercises
                    .Where(te => nameToId.ContainsKey(te.ExerciseName))
                    .Select(te => new TemplateExercise
                    {
                        TemplateId = template.Id,
                        ExerciseId = nameToId[te.ExerciseName],
                        OrderIndex = te.OrderIndex,
                        DefaultSets = te.DefaultSets,
                        DefaultReps = te.DefaultReps
                    }).ToList();

                await _context.TemplateExercises.AddRangeAsync(templateExercises);
            }

            await _context.SaveChangesAsync();
        }

        private static SeedFile LoadSeedFile()
        {
            if (_seedFile is not null)
                return _seedFile;

            lock (_lock)
            {
                if (_seedFile is not null)
                    return _seedFile;

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .First(n => n.EndsWith("seed-data.json", StringComparison.OrdinalIgnoreCase));

                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                _seedFile = JsonSerializer.Deserialize<SeedFile>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }

            return _seedFile;
        }

        private sealed record SeedFile(
            [property: JsonPropertyName("exercises")] List<SeedExercise> Exercises,
            [property: JsonPropertyName("templates")] List<SeedTemplate> Templates);

        private sealed record SeedExercise(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("muscleGroup")] string MuscleGroup,
            [property: JsonPropertyName("notes")] string Notes);

        private sealed record SeedTemplate(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("description")] string Description,
            [property: JsonPropertyName("dayOfWeek")] string DayOfWeek,
            [property: JsonPropertyName("exercises")] List<SeedTemplateExercise> Exercises);

        private sealed record SeedTemplateExercise(
            [property: JsonPropertyName("exerciseName")] string ExerciseName,
            [property: JsonPropertyName("orderIndex")] int OrderIndex,
            [property: JsonPropertyName("defaultSets")] int DefaultSets,
            [property: JsonPropertyName("defaultReps")] int DefaultReps);
    }
}
