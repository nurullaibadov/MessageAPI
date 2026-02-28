using MessageAPI.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessageAPI.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        protected string CurrentUserEmail =>
            User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        protected bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Generic overload — Result<T> için
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            return result.StatusCode switch
            {
                200 => Ok(ApiResponse.Ok(result.Data)),
                201 => StatusCode(201, ApiResponse.Ok(result.Data)),
                400 => BadRequest(ApiResponse.Fail(result.Error, result.Errors)),
                401 => Unauthorized(ApiResponse.Fail(result.Error)),
                403 => StatusCode(403, ApiResponse.Fail(result.Error)),
                404 => NotFound(ApiResponse.Fail(result.Error ?? "Not found")),
                _ => StatusCode(500, ApiResponse.Fail(result.Error ?? "Server error"))
            };
        }

        // Non-generic overload — Result için (Logout, Delete, MarkRead vb.)
        protected IActionResult HandleResult(Result result)
        {
            return result.StatusCode switch
            {
                200 => Ok(ApiResponse.Ok<object>(null)),
                201 => StatusCode(201, ApiResponse.Ok<object>(null)),
                400 => BadRequest(ApiResponse.Fail(result.Error, result.Errors)),
                401 => Unauthorized(ApiResponse.Fail(result.Error)),
                403 => StatusCode(403, ApiResponse.Fail(result.Error)),
                404 => NotFound(ApiResponse.Fail(result.Error ?? "Not found")),
                _ => StatusCode(500, ApiResponse.Fail(result.Error ?? "Server error"))
            };
        }
    }

    // Statik factory — type inference sorununu ortadan kaldırır
    public static class ApiResponse
    {
        public static ApiResponse<T> Ok<T>(T? data, string message = "Success")
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<object> Fail(string? error, List<string>? errors = null)
            => new() { Success = false, Message = error ?? "Error", Errors = errors ?? new List<string>() };
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
