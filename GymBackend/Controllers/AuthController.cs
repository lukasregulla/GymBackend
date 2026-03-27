using GymBackend.Service;
using GymBackend.Model.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GymBackend.Exceptions;

namespace GymBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                var result = await _authService.Register(registerRequest);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
[HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var result = await _authService.Login(loginRequest);
                return Ok(result);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
