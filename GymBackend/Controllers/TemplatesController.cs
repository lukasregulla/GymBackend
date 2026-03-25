using System.Security.Claims;
using GymBackend.Exceptions;
using GymBackend.Model.Dto.Template;
using GymBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TemplatesController(ITemplateService templateService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await templateService.GetAllAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { return Ok(await templateService.GetByIdAsync(id, UserId)); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto dto)
    {
        var result = await templateService.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTemplateDto dto)
    {
        try { return Ok(await templateService.UpdateAsync(id, dto, UserId)); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await templateService.DeleteAsync(id, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("{id}/exercises")]
    public async Task<IActionResult> AddExercise(int id, [FromBody] AddTemplateExerciseDto dto)
    {
        try
        {
            var result = await templateService.AddExerciseAsync(id, dto, UserId);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (BadRequestException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id}/exercises/{exerciseId}")]
    public async Task<IActionResult> RemoveExercise(int id, int exerciseId)
    {
        try
        {
            await templateService.RemoveExerciseAsync(id, exerciseId, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id}/exercises/reorder")]
    public async Task<IActionResult> ReorderExercises(int id, [FromBody] ReorderExercisesDto dto)
    {
        try
        {
            await templateService.ReorderExercisesAsync(id, dto, UserId);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
