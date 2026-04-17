
namespace GymBackend.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public ICollection<Exercise> exercises { get; set; } = new List<Exercise>();
        public ICollection<WorkoutTemplate> workoutTemplates { get; set; } = new List<WorkoutTemplate>();
        public ICollection<WorkoutSession> workoutSessions { get; set; } = new List<WorkoutSession>();

    }
}
