using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    [Authorize]
    public class ConversationsController : BaseController
    {
        private readonly IConversationService _conversationService;

        public ConversationsController(IConversationService conversationService)
            => _conversationService = conversationService;

        /// <summary>Get all conversations of current user</summary>
        [HttpGet]
        public async Task<IActionResult> GetConversations()
            => HandleResult(await _conversationService.GetUserConversationsAsync(CurrentUserId));

        /// <summary>Get single conversation</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetConversation(Guid id)
            => HandleResult(await _conversationService.GetConversationAsync(id, CurrentUserId));

        /// <summary>Start or get private conversation with user</summary>
        [HttpPost("private/{targetUserId:guid}")]
        public async Task<IActionResult> GetOrCreatePrivate(Guid targetUserId)
            => HandleResult(await _conversationService.GetOrCreatePrivateConversationAsync(CurrentUserId, targetUserId));

        /// <summary>Create group conversation</summary>
        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
            => HandleResult(await _conversationService.CreateGroupAsync(CurrentUserId, dto));

        /// <summary>Update group info</summary>
        [HttpPut("{id:guid}/groups")]
        public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupDto dto)
            => HandleResult(await _conversationService.UpdateGroupAsync(id, CurrentUserId, dto));

        /// <summary>Add participant to group</summary>
        [HttpPost("{id:guid}/participants/{userId:guid}")]
        public async Task<IActionResult> AddParticipant(Guid id, Guid userId)
            => HandleResult(await _conversationService.AddParticipantAsync(id, CurrentUserId, userId));

        /// <summary>Remove participant from group</summary>
        [HttpDelete("{id:guid}/participants/{userId:guid}")]
        public async Task<IActionResult> RemoveParticipant(Guid id, Guid userId)
            => HandleResult(await _conversationService.RemoveParticipantAsync(id, CurrentUserId, userId));

        /// <summary>Leave conversation</summary>
        [HttpPost("{id:guid}/leave")]
        public async Task<IActionResult> Leave(Guid id)
            => HandleResult(await _conversationService.LeaveConversationAsync(id, CurrentUserId));

        /// <summary>Delete conversation</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => HandleResult(await _conversationService.DeleteConversationAsync(id, CurrentUserId));
    }
}
