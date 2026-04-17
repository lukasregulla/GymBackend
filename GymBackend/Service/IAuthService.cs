using GymBackend.Model;
using GymBackend.Model.Dto.Auth;

namespace GymBackend.Service
{
    public interface IAuthService
    {

        string GenerateJwtToken(User user);
        Task<AuthResponseDto> Login(LoginRequestDto loginRequest);
        Task<AuthResponseDto> Register(RegisterRequestDto registerRequest);
        Task<bool> DeleteUserByEmail(string email);
        Task ConfirmEmailAsync(ConfirmEmailRequestDto request);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
