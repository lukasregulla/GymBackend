using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
