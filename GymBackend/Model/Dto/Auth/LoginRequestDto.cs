using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;
    }
}
