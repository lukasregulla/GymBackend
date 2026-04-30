using System.Security.Cryptography;
using System.Text;
using GymBackend.Repositories;

namespace GymBackend.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarRepository _calendarRepository;

        public CalendarService(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<string> GenerateScheduleIcsAsync(int userId)
        {
            var sessions = await _calendarRepository.GetUpcomingIncompleteScheduledSessionsAsync(userId);

            return BuildIcsContent(sessions);
        }

        public async Task<string?> GenerateScheduleIcsFromTokenAsync(string token)
        {
            var user = await _calendarRepository.GetUserByCalendarTokenAsync(token);

            if (user == null)
            {
                return null;
            }

            return await GenerateScheduleIcsAsync(user.Id);
        }

        public async Task<string?> GetOrCreateSubscriptionUrlAsync(int userId, string baseUrl)
        {
            var user = await _calendarRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(user.CalendarSubscriptionToken))
            {
                user.CalendarSubscriptionToken = GenerateCalendarToken();
                await _calendarRepository.SaveChangesAsync();
            }

            return $"{baseUrl}/api/calendar/subscribe/{user.CalendarSubscriptionToken}.ics";
        }

        public async Task<string?> ResetSubscriptionUrlAsync(int userId, string baseUrl)
        {
            var user = await _calendarRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            user.CalendarSubscriptionToken = GenerateCalendarToken();
            await _calendarRepository.SaveChangesAsync();

            return $"{baseUrl}/api/calendar/subscribe/{user.CalendarSubscriptionToken}.ics";
        }

        private static string BuildIcsContent(IEnumerable<Model.WorkoutSession> sessions)
        {
            var sb = new StringBuilder();

            AppendIcsLine(sb, "BEGIN:VCALENDAR");
            AppendIcsLine(sb, "VERSION:2.0");
            AppendIcsLine(sb, "PRODID:-//GymTracker//Workout Schedule//EN");
            AppendIcsLine(sb, "CALSCALE:GREGORIAN");
            AppendIcsLine(sb, "METHOD:PUBLISH");

            foreach (var session in sessions)
            {
                var startDateTime = session.ScheduledDate!.Value.ToDateTime(session.ScheduledStartTime!.Value);

                var durationMinutes = session.EstimatedDurationMinutes ?? 60;
                var endDateTime = startDateTime.AddMinutes(durationMinutes);

                var title = string.IsNullOrWhiteSpace(session.Name)
                    ? "Workout Session"
                    : session.Name;

                var exerciseLines = session.SessionExercises
                    .OrderBy(se => se.Id)
                    .Select(se => $"- {se.Exercise.Name}")
                    .ToList();

                var description = exerciseLines.Any()
                    ? $"Exercises:\n{string.Join("\n", exerciseLines)}"
                    : "No exercises added yet.";

                AppendIcsLine(sb, "BEGIN:VEVENT");
                AppendIcsLine(sb, $"UID:{session.Id}@gymtracker");
                AppendIcsLine(sb, $"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}");
                AppendIcsLine(sb, $"DTSTART:{startDateTime:yyyyMMdd'T'HHmmss}");
                AppendIcsLine(sb, $"DTEND:{endDateTime:yyyyMMdd'T'HHmmss}");
                AppendIcsLine(sb, $"SUMMARY:{EscapeIcsText(title)}");
                AppendIcsLine(sb, $"DESCRIPTION:{EscapeIcsText(description)}");
                AppendIcsLine(sb, "END:VEVENT");
            }

            AppendIcsLine(sb, "END:VCALENDAR");

            return sb.ToString();
        }

        private static void AppendIcsLine(StringBuilder sb, string line)
        {
            sb.Append(line).Append("\r\n");
        }

        private static string GenerateCalendarToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string EscapeIcsText(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }
    }
}