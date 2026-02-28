using MessageAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public Message? ReplyToMessage { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        // Navigation
        public ICollection<MessageReadStatus> ReadStatuses { get; set; } = new List<MessageReadStatus>();
        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    }
}
