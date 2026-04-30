using System.Security.Claims;
using GymBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet("schedule.ics")]
        [Authorize]
        public async Task<IActionResult> ExportSchedule()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var icsContent = await _calendarService.GenerateScheduleIcsAsync(userId.Value);

            return File(
                System.Text.Encoding.UTF8.GetBytes(icsContent),
                "text/calendar",
                "gym-schedule.ics"
            );
        }

        [HttpGet("subscription-link")]
        [Authorize]
        public async Task<IActionResult> GetSubscriptionLink()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var subscriptionUrl = await _calendarService.GetOrCreateSubscriptionUrlAsync(
                userId.Value,
                baseUrl
            );

            if (subscriptionUrl == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                calendarUrl = subscriptionUrl
            });
        }

        [HttpPost("reset-subscription-link")]
        [Authorize]
        public async Task<IActionResult> ResetSubscriptionLink()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var subscriptionUrl = await _calendarService.ResetSubscriptionUrlAsync(
                userId.Value,
                baseUrl
            );

            if (subscriptionUrl == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                calendarUrl = subscriptionUrl
            });
        }

        [HttpGet("subscribe/{token}.ics")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe(string token)
        {
            var icsContent = await _calendarService.GenerateScheduleIcsFromTokenAsync(token);

            if (icsContent == null)
            {
                return NotFound();
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return File(
                System.Text.Encoding.UTF8.GetBytes(icsContent),
                "text/calendar",
                "gym-schedule.ics"
            );
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}