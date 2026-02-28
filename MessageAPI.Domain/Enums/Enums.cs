using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Enums
{
    public enum UserStatus { Online, Away, Busy, Offline }
    public enum ConversationType { Private, Group, Channel }
    public enum MessageType { Text, Image, File, Audio, Video, System }
    public enum MessageStatus { Sent, Delivered, Read }
    public enum ParticipantRole { Member, Admin, Owner }
    public enum FriendshipStatus { Pending, Accepted, Blocked, Declined }
    public enum NotificationType { Message, FriendRequest, GroupInvite, SystemAlert }
}
