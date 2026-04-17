using GymBackend.Data;
using GymBackend.Interfaces;
using GymBackend.Model;
using Microsoft.EntityFrameworkCore;
using GymBackend.Model.Dto.Auth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GymBackend.Exceptions;

namespace GymBackend.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ISeedDataService _seedDataService;
        private readonly IEmailService _emailService;

        public AuthService(IConfiguration configuration, AppDbContext context, ISeedDataService seedDataService, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _seedDataService = seedDataService;
            _emailService = emailService;
        }

        private static string HashToken(string rawToken)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        public async Task<AuthResponseDto> Register(RegisterRequestDto registerRequest)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            if (existingUser != null)
            {
                throw new BadRequestException("User with this email already exists.");
            }
            var existingUsername = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerRequest.Username);
            if (existingUsername != null)
            {
                throw new BadRequestException("User with this username already exists.");
            }
                var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _seedDataService.SeedForUserAsync(user.Id);

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.EmailConfirmationToken = HashToken(rawToken);
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(1);
            await _context.SaveChangesAsync();

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var link = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";
            await _emailService.SendAsync(user.Email, "Confirm your email", $"Please confirm your email: {link}");

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Username = user.Username
            };
        }

        public async Task<AuthResponseDto> Login(LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }
            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedException("Please confirm your email before logging in.");
            }
            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Username = user.Username
            };
        }

        public async Task ConfirmEmailAsync(ConfirmEmailRequestDto request)
        {
            if (!int.TryParse(request.UserId, out var userId))
            {
                throw new BadRequestException("Invalid confirmation link.");
            }

            var user = await _context.Users.FindAsync(userId);
            var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            if (user == null
                || user.EmailConfirmationToken == null
                || user.EmailConfirmationToken != HashToken(rawToken)
                || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            {
                throw new BadRequestException("Invalid confirmation link.");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;
            await _context.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return;
            }

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.PasswordResetToken = HashToken(rawToken);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var link = $"{_configuration["Frontend:BaseUrl"]}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={encodedToken}";
            await _emailService.SendAsync(user.Email, "Reset your password", $"Reset your password: {link}");
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            if (user == null
                || user.PasswordResetToken == null
                || user.PasswordResetToken != HashToken(rawToken)
                || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                throw new BadRequestException("Invalid or expired reset link.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

    }
}
