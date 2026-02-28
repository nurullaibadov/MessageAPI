using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastSeen { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    // DTOs/Users/UpdateProfileDto.cs
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Bio { get; set; }
    }

    // DTOs/Admin/AdminUserDto.cs

    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastSeen { get; set; }
        public List<string> Roles { get; set; } = new();
        public int MessageCount { get; set; }
    }

    // DTOs/Admin/DashboardStatsDto.cs
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int TotalMessages { get; set; }
        public int TotalConversations { get; set; }
        public int TodayMessages { get; set; }
        public int TodayNewUsers { get; set; }
        public List<DailyStatDto> Last7DaysStats { get; set; } = new();
    }

    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public int Messages { get; set; }
        public int NewUsers { get; set; }
    }
}
