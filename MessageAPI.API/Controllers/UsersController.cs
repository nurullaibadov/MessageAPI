using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;
        private readonly INotificationService _notificationService;

        public UsersController(IUserService userService, IFriendshipService friendshipService,
            INotificationService notificationService)
        {
            _userService = userService;
            _friendshipService = friendshipService;
            _notificationService = notificationService;
        }

        /// <summary>Search users</summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
            => HandleResult(await _userService.SearchUsersAsync(q, CurrentUserId));

        /// <summary>Get user by id</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => HandleResult(await _userService.GetUserByIdAsync(id));

        /// <summary>Get user by username</summary>
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
            => HandleResult(await _userService.GetUserByUsernameAsync(username));

        /// <summary>Update my profile</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
            => HandleResult(await _userService.UpdateProfileAsync(CurrentUserId, dto));

        /// <summary>Update online status</summary>
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
            => HandleResult(await _userService.UpdateStatusAsync(CurrentUserId, dto.Status));

        /// <summary>Deactivate my account</summary>
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate()
            => HandleResult(await _userService.DeactivateAccountAsync(CurrentUserId));

        // Friends
        /// <summary>Get my friends list</summary>
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
            => HandleResult(await _friendshipService.GetFriendsAsync(CurrentUserId));

        /// <summary>Get pending friend requests</summary>
        [HttpGet("friends/pending")]
        public async Task<IActionResult> GetPendingRequests()
            => HandleResult(await _friendshipService.GetPendingRequestsAsync(CurrentUserId));

        /// <summary>Send friend request</summary>
        [HttpPost("friends/{targetId:guid}")]
        public async Task<IActionResult> SendFriendRequest(Guid targetId)
            => HandleResult(await _friendshipService.SendFriendRequestAsync(CurrentUserId, targetId));

        /// <summary>Accept friend request</summary>
        [HttpPost("friends/{requesterId:guid}/accept")]
        public async Task<IActionResult> AcceptRequest(Guid requesterId)
            => HandleResult(await _friendshipService.AcceptFriendRequestAsync(CurrentUserId, requesterId));

        /// <summary>Decline friend request</summary>
        [HttpPost("friends/{requesterId:guid}/decline")]
        public async Task<IActionResult> DeclineRequest(Guid requesterId)
            => HandleResult(await _friendshipService.DeclineFriendRequestAsync(CurrentUserId, requesterId));

        /// <summary>Remove friend</summary>
        [HttpDelete("friends/{friendId:guid}")]
        public async Task<IActionResult> RemoveFriend(Guid friendId)
            => HandleResult(await _friendshipService.RemoveFriendAsync(CurrentUserId, friendId));

        /// <summary>Block user</summary>
        [HttpPost("block/{targetId:guid}")]
        public async Task<IActionResult> BlockUser(Guid targetId)
            => HandleResult(await _friendshipService.BlockUserAsync(CurrentUserId, targetId));

        // Notifications
        /// <summary>Get my notifications</summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
            => HandleResult(await _notificationService.GetNotificationsAsync(CurrentUserId, unreadOnly));

        /// <summary>Get unread notification count</summary>
        [HttpGet("notifications/count")]
        public async Task<IActionResult> GetUnreadCount()
            => HandleResult(await _notificationService.GetUnreadCountAsync(CurrentUserId));

        /// <summary>Mark notification as read</summary>
        [HttpPost("notifications/{id:guid}/read")]
        public async Task<IActionResult> MarkNotificationRead(Guid id)
            => HandleResult(await _notificationService.MarkAsReadAsync(id, CurrentUserId));

        /// <summary>Mark all notifications as read</summary>
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
            => HandleResult(await _notificationService.MarkAllAsReadAsync(CurrentUserId));
    }

    public class UpdateStatusDto { public string Status { get; set; } = string.Empty; }
}
