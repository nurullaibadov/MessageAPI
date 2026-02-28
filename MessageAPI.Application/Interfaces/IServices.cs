using MessageAPI.Application.DTOs;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<Result> LogoutAsync(Guid userId);
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<Result> VerifyEmailAsync(string token, string email);
        Task<Result> ResendVerificationEmailAsync(string email);
    }

    // Interfaces/IMessageService.cs
    public interface IMessageService
    {
        Task<Result<MessageDto>> SendMessageAsync(Guid senderId, SendMessageDto dto);
        Task<Result<PagedResult<MessageDto>>> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize);
        Task<Result<MessageDto>> EditMessageAsync(Guid messageId, Guid userId, EditMessageDto dto);
        Task<Result> DeleteMessageAsync(Guid messageId, Guid userId);
        Task<Result> MarkAsReadAsync(Guid conversationId, Guid userId);
        Task<Result<MessageDto>> AddReactionAsync(Guid messageId, Guid userId, string emoji);
        Task<Result> RemoveReactionAsync(Guid messageId, Guid userId, string emoji);
    }

    // Interfaces/IConversationService.cs
    public interface IConversationService
    {
        Task<Result<ConversationDto>> GetOrCreatePrivateConversationAsync(Guid userId, Guid targetUserId);
        Task<Result<ConversationDto>> CreateGroupAsync(Guid creatorId, CreateGroupDto dto);
        Task<Result<IEnumerable<ConversationDto>>> GetUserConversationsAsync(Guid userId);
        Task<Result<ConversationDto>> GetConversationAsync(Guid conversationId, Guid userId);
        Task<Result> AddParticipantAsync(Guid conversationId, Guid requesterId, Guid newUserId);
        Task<Result> RemoveParticipantAsync(Guid conversationId, Guid requesterId, Guid userId);
        Task<Result> LeaveConversationAsync(Guid conversationId, Guid userId);
        Task<Result> UpdateGroupAsync(Guid conversationId, Guid userId, UpdateGroupDto dto);
        Task<Result> DeleteConversationAsync(Guid conversationId, Guid userId);
    }

    // Interfaces/IUserService.cs
    public interface IUserService
    {
        Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
        Task<Result<UserDto>> GetUserByUsernameAsync(string username);
        Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<Result<IEnumerable<UserDto>>> SearchUsersAsync(string query, Guid currentUserId);
        Task<Result> UpdateStatusAsync(Guid userId, string status);
        Task<Result> DeactivateAccountAsync(Guid userId);
    }

    // Interfaces/IAdminService.cs
    public interface IAdminService
    {
        Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<Result<PagedResult<AdminUserDto>>> GetAllUsersAsync(int page, int pageSize, string? search);
        Task<Result<AdminUserDto>> GetUserDetailsAsync(Guid userId);
        Task<Result> BanUserAsync(Guid userId, string reason);
        Task<Result> UnbanUserAsync(Guid userId);
        Task<Result> AssignRoleAsync(Guid userId, string role);
        Task<Result> RemoveRoleAsync(Guid userId, string role);
        Task<Result> DeleteUserAsync(Guid userId);
        Task<Result<IEnumerable<ConversationDto>>> GetAllConversationsAsync(int page, int pageSize);
    }

    // Interfaces/IFriendshipService.cs
    public interface IFriendshipService
    {
        Task<Result> SendFriendRequestAsync(Guid requesterId, Guid addresseeId);
        Task<Result> AcceptFriendRequestAsync(Guid userId, Guid requesterId);
        Task<Result> DeclineFriendRequestAsync(Guid userId, Guid requesterId);
        Task<Result> RemoveFriendAsync(Guid userId, Guid friendId);
        Task<Result> BlockUserAsync(Guid userId, Guid targetId);
        Task<Result<IEnumerable<UserDto>>> GetFriendsAsync(Guid userId);
        Task<Result<IEnumerable<UserDto>>> GetPendingRequestsAsync(Guid userId);
    }

    // Interfaces/INotificationService.cs
    public interface INotificationService
    {
        Task<Result<IEnumerable<NotificationDto>>> GetNotificationsAsync(Guid userId, bool unreadOnly = false);
        Task<Result<int>> GetUnreadCountAsync(Guid userId);
        Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<Result> MarkAllAsReadAsync(Guid userId);
        Task SendNotificationAsync(Guid userId, string title, string content, NotificationType type, string? referenceId = null);
    }

    // Interfaces/IJwtService.cs
    public interface IJwtService
    {
        string GenerateAccessToken(User user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Guid GetUserIdFromToken(string token);
    }

    // Interfaces/IEmailService.cs
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendPasswordResetEmailAsync(string to, string token, string email);
        Task SendEmailVerificationAsync(string to, string token, string email);
        Task SendWelcomeEmailAsync(string to, string username);
    }

    // Common/PagedResult.cs

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
