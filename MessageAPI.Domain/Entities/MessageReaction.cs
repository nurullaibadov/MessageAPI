using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Entities
{
    public class MessageReaction : BaseEntity
    {
        public Guid MessageId { get; set; }
        public Message Message { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Emoji { get; set; } = string.Empty;
    }
}
