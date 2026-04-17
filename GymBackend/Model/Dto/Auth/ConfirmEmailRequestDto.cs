using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Auth
{
    public class ConfirmEmailRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
