using MessageAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public string? Name { get; set; }
        public ConversationType Type { get; set; } = ConversationType.Private;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime? LastMessageAt { get; set; }

        // Navigation
        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
