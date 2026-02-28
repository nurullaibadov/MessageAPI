using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>Register new user</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
            => HandleResult(await _authService.RegisterAsync(dto));

        /// <summary>Login</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
            => HandleResult(await _authService.LoginAsync(dto));

        /// <summary>Refresh access token</summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
            => HandleResult(await _authService.RefreshTokenAsync(dto));

        /// <summary>Logout</summary>
        [HttpPost("logout"), Authorize]
        public async Task<IActionResult> Logout()
            => HandleResult(await _authService.LogoutAsync(CurrentUserId));

        /// <summary>Forgot password - sends email</summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
            => HandleResult(await _authService.ForgotPasswordAsync(dto));

        /// <summary>Reset password with token</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
            => HandleResult(await _authService.ResetPasswordAsync(dto));

        /// <summary>Change password (authenticated)</summary>
        [HttpPost("change-password"), Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
            => HandleResult(await _authService.ChangePasswordAsync(CurrentUserId, dto));

        /// <summary>Verify email</summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
            => HandleResult(await _authService.VerifyEmailAsync(token, email));

        /// <summary>Resend verification email</summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto dto)
            => HandleResult(await _authService.ResendVerificationEmailAsync(dto.Email));

        /// <summary>Get current user info</summary>
        [HttpGet("me"), Authorize]
        public async Task<IActionResult> Me([FromServices] IUserService userService)
            => HandleResult(await userService.GetUserByIdAsync(CurrentUserId));
    }
}
