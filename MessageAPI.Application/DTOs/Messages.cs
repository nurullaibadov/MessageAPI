using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Application.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfilePicture { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public string? ReplyToContent { get; set; }
        public bool IsEdited { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ReactionDto> Reactions { get; set; } = new();
        public bool IsMyMessage { get; set; }
    }

    // DTOs/Messages/SendMessageDto.cs
    public class SendMessageDto
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "Text";
        public Guid? ReplyToMessageId { get; set; }
    }

    // DTOs/Messages/EditMessageDto.cs
    public class EditMessageDto { public string Content { get; set; } = string.Empty; }

    // DTOs/Messages/ReactionDto.cs
    public class ReactionDto
    {
        public string Emoji { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsMyReaction { get; set; }
    }

    // DTOs/Conversations/ConversationDto.cs

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public List<ParticipantDto> Participants { get; set; } = new();
        public DateTime? LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTOs/Conversations/CreateGroupDto.cs
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
    }

    // DTOs/Conversations/ParticipantDto.cs
    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
