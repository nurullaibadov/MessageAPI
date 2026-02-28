using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService) => _adminService = adminService;

        // ─── DASHBOARD ───────────────────────────────────────────────

        /// <summary>Admin dashboard stats</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
            => HandleResult(await _adminService.GetDashboardStatsAsync());

        // ─── USERS ───────────────────────────────────────────────────

        /// <summary>Get all users (paginated)</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
            => HandleResult(await _adminService.GetAllUsersAsync(page, pageSize, search));

        /// <summary>Get user detail</summary>
        [HttpGet("users/{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
            => HandleResult(await _adminService.GetUserDetailsAsync(id));

        /// <summary>Ban user</summary>
        [HttpPost("users/{id:guid}/ban")]
        public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserDto dto)
            => HandleResult(await _adminService.BanUserAsync(id, dto.Reason));

        /// <summary>Unban user</summary>
        [HttpPost("users/{id:guid}/unban")]
        public async Task<IActionResult> UnbanUser(Guid id)
            => HandleResult(await _adminService.UnbanUserAsync(id));

        /// <summary>Delete user (soft delete)</summary>
        [HttpDelete("users/{id:guid}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> DeleteUser(Guid id)
            => HandleResult(await _adminService.DeleteUserAsync(id));

        // ─── ROLES ───────────────────────────────────────────────────

        /// <summary>Assign role to user</summary>
        [HttpPost("users/{id:guid}/roles")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] RoleDto dto)
            => HandleResult(await _adminService.AssignRoleAsync(id, dto.Role));

        /// <summary>Remove role from user</summary>
        [HttpDelete("users/{id:guid}/roles")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> RemoveRole(Guid id, [FromBody] RoleDto dto)
            => HandleResult(await _adminService.RemoveRoleAsync(id, dto.Role));

        // ─── CONVERSATIONS ────────────────────────────────────────────

        /// <summary>Get all conversations</summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => HandleResult(await _adminService.GetAllConversationsAsync(page, pageSize));
    }

    public class BanUserDto { public string Reason { get; set; } = string.Empty; }
    public class RoleDto { public string Role { get; set; } = string.Empty; }
}
