using System.Security.Claims;
using GymBackend.Exceptions;
using GymBackend.Model.Dto.Session;
using GymBackend.Model.Dto.Set;
using GymBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SessionsController(ISessionService sessionService, ISetService setService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? from, [FromQuery] DateOnly? to) =>
        Ok(await sessionService.GetAllAsync(UserId, from, to));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { return Ok(await sessionService.GetByIdAsync(id, UserId)); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        try
        {
            var result = await sessionService.CreateAsync(dto, UserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        try { return Ok(await sessionService.CompleteAsync(id, UserId)); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await sessionService.DeleteAsync(id, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("{id}/exercises")]
    public async Task<IActionResult> AddExercise(int id, [FromBody] AddSessionExerciseDto dto)
    {
        try
        {
            var result = await sessionService.AddExerciseAsync(id, dto, UserId);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id}/exercises/{sessionExerciseId}")]
    public async Task<IActionResult> RemoveExercise(int id, int sessionExerciseId)
    {
        try
        {
            await sessionService.RemoveExerciseAsync(id, sessionExerciseId, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("{sessionId}/exercises/{sessionExerciseId}/sets")]
    public async Task<IActionResult> LogSet(int sessionId, int sessionExerciseId, [FromBody] LogSetDto dto)
    {
        try
        {
            var result = await setService.LogSetAsync(sessionId, sessionExerciseId, dto, UserId);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{sessionId}/exercises/{sessionExerciseId}/sets/{setId}")]
    public async Task<IActionResult> UpdateSet(int sessionId, int sessionExerciseId, int setId, [FromBody] UpdateSetDto dto)
    {
        try
        {
            var result = await setService.UpdateSetAsync(sessionId, sessionExerciseId, setId, dto, UserId);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{sessionId}/exercises/{sessionExerciseId}/sets/{setId}")]
    public async Task<IActionResult> DeleteSet(int sessionId, int sessionExerciseId, int setId)
    {
        try
        {
            await setService.DeleteSetAsync(sessionId, sessionExerciseId, setId, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
