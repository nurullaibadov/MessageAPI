using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService) => _messageService = messageService;

        /// <summary>Get messages in a conversation (paginated)</summary>
        [HttpGet("conversations/{conversationId:guid}")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
            => HandleResult(await _messageService.GetMessagesAsync(conversationId, CurrentUserId, page, pageSize));

        /// <summary>Send message</summary>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
            => HandleResult(await _messageService.SendMessageAsync(CurrentUserId, dto));

        /// <summary>Edit message</summary>
        [HttpPut("{messageId:guid}")]
        public async Task<IActionResult> Edit(Guid messageId, [FromBody] EditMessageDto dto)
            => HandleResult(await _messageService.EditMessageAsync(messageId, CurrentUserId, dto));

        /// <summary>Delete message (soft)</summary>
        [HttpDelete("{messageId:guid}")]
        public async Task<IActionResult> Delete(Guid messageId)
            => HandleResult(await _messageService.DeleteMessageAsync(messageId, CurrentUserId));

        /// <summary>Mark conversation messages as read</summary>
        [HttpPost("conversations/{conversationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid conversationId)
            => HandleResult(await _messageService.MarkAsReadAsync(conversationId, CurrentUserId));

        /// <summary>Add reaction to message</summary>
        [HttpPost("{messageId:guid}/reactions")]
        public async Task<IActionResult> AddReaction(Guid messageId, [FromBody] AddReactionDto dto)
            => HandleResult(await _messageService.AddReactionAsync(messageId, CurrentUserId, dto.Emoji));

        /// <summary>Remove reaction from message</summary>
        [HttpDelete("{messageId:guid}/reactions/{emoji}")]
        public async Task<IActionResult> RemoveReaction(Guid messageId, string emoji)
            => HandleResult(await _messageService.RemoveReactionAsync(messageId, CurrentUserId, emoji));
    }

    public class AddReactionDto { public string Emoji { get; set; } = string.Empty; }
}
