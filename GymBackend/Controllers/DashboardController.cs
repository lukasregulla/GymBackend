using System.Security.Claims;
using GymBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("week")]
    public async Task<IActionResult> GetWeek() =>
        Ok(await dashboardService.GetWeekAsync(UserId));

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 5) =>
        Ok(await dashboardService.GetRecentAsync(UserId, count));
}
