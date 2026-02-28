using MessageAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Entities
{
    public class Friendship : BaseEntity
    {
        public Guid RequesterId { get; set; }
        public User Requester { get; set; } = null!;
        public Guid AddresseeId { get; set; }
        public User Addressee { get; set; } = null!;
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
        public DateTime? RespondedAt { get; set; }
    }
}
