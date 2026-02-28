using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<User> userManager, IJwtService jwtService,
            IEmailService emailService, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Result<AuthResponseDto>.Failure("Email already registered.");

            if (await _userManager.FindByNameAsync(dto.Username) != null)
                return Result<AuthResponseDto>.Failure("Username already taken.");

            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailVerificationToken = Guid.NewGuid().ToString("N"),
                IsActive = true,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Failure(result.Errors.Select(e => e.Description).ToList());

            await _userManager.AddToRoleAsync(user, "User");

            try { await _emailService.SendEmailVerificationAsync(user.Email!, user.EmailVerificationToken!, user.Email!); }
            catch { /* Email failure should not block registration */ }

            return await BuildAuthResponse(user);
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = dto.EmailOrUsername.Contains('@')
                ? await _userManager.FindByEmailAsync(dto.EmailOrUsername)
                : await _userManager.FindByNameAsync(dto.EmailOrUsername);

            if (user == null || user.IsDeleted)
                return Result<AuthResponseDto>.Failure("Invalid credentials.");

            if (!user.IsActive)
                return Result<AuthResponseDto>.Failure("Account is deactivated. Please contact support.");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Result<AuthResponseDto>.Failure("Invalid credentials.");

            return await BuildAuthResponse(user);
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return Result<AuthResponseDto>.Unauthorized("Invalid token.");

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return Result<AuthResponseDto>.Unauthorized("Invalid or expired refresh token.");

            return await BuildAuthResponse(user);
        }

        public async Task<Result> LogoutAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.");
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Result.Success(); // Güvenlik için her zaman success döndür

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _userManager.UpdateAsync(user);

            try { await _emailService.SendPasswordResetEmailAsync(user.Email!, token, user.Email!); }
            catch { /* log */ }

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result.Failure("Invalid request.");

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return Result.Failure("Reset token has expired.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return Result.Failure(result.Errors.Select(e => e.Description).ToList().FirstOrDefault() ?? "Reset failed.");

            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return Result.Failure(result.Errors.Select(e => e.Description).FirstOrDefault() ?? "Change failed.");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> VerifyEmailAsync(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Result.Failure("User not found.", 404);
            if (user.EmailVerificationToken != token) return Result.Failure("Invalid token.");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> ResendVerificationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Result.Success();
            if (user.IsEmailVerified) return Result.Failure("Email already verified.");

            user.EmailVerificationToken = Guid.NewGuid().ToString("N");
            await _userManager.UpdateAsync(user);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.EmailVerificationToken!, user.Email!);
            return Result.Success();
        }

        private async Task<Result<AuthResponseDto>> BuildAuthResponse(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Roles = roles.ToList()
            });
        }
    }
}
