using System.ComponentModel.DataAnnotations;

namespace GymBackend.Model.Dto.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;
    }
}
