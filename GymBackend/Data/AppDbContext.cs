
using System.Reflection.Emit;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace GymBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<WorkoutTemplate> WorkoutTemplates { get; set; } = null!;
        public DbSet<TemplateExercise> TemplateExercises { get; set; } = null!;
        public DbSet<WorkoutSession> WorkoutSessions { get; set; } = null!;
        public DbSet<SessionExercise> SessionExercises { get; set; } = null!;
        public DbSet<Set> Sets { get; set; } = null!;
        public DbSet<RunDetail> RunDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Explicitly configure the nullable TemplateId on WorkoutSession
            modelBuilder.Entity<WorkoutSession>()
                .HasOne(ws => ws.Template)
                .WithMany(t => t.WorkoutSessions)
                .HasForeignKey(ws => ws.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            // TemplateExercise join — tell EF both FKs explicitly
            modelBuilder.Entity<TemplateExercise>()
                .HasOne(te => te.Template)
                .WithMany(t => t.TemplateExercises)
                .HasForeignKey(te => te.TemplateId);

            modelBuilder.Entity<TemplateExercise>()
                .HasOne(te => te.Exercise)
                .WithMany(e => e.TemplateExercises)
                .HasForeignKey(te => te.ExerciseId);

            // SessionExercise join — same pattern
            modelBuilder.Entity<SessionExercise>()
                .HasOne(se => se.Session)
                .WithMany(s => s.SessionExercises)
                .HasForeignKey(se => se.SessionId);

            modelBuilder.Entity<SessionExercise>()
                .HasOne(se => se.Exercise)
                .WithMany(e => e.SessionExercises)
                .HasForeignKey(se => se.ExerciseId);

            // One-to-one: WorkoutSession → RunDetail
            modelBuilder.Entity<WorkoutSession>()
                .HasOne(ws => ws.RunDetail)
                .WithOne(rd => rd.WorkoutSession)
                .HasForeignKey<RunDetail>(rd => rd.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
