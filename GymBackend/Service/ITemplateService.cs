using GymBackend.Model.Dto.Template;

namespace GymBackend.Service;

public interface ITemplateService
{
    Task<List<TemplateDto>> GetAllAsync(int userId);
    Task<TemplateDetailDto> GetByIdAsync(int id, int userId);
    Task<TemplateDto> CreateAsync(CreateTemplateDto dto, int userId);
    Task<TemplateDto> UpdateAsync(int id, UpdateTemplateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task<TemplateExerciseDto> AddExerciseAsync(int templateId, AddTemplateExerciseDto dto, int userId);
    Task RemoveExerciseAsync(int templateId, int exerciseId, int userId);
    Task ReorderExercisesAsync(int templateId, ReorderExercisesDto dto, int userId);
}
