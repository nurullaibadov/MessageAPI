using AutoMapper;
using MessageAPI.Application.DTOs;
using MessageAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Roles, o => o.Ignore());

            CreateMap<User, AdminUserDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(d => d.Roles, o => o.Ignore());

            // Message mappings
            CreateMap<Message, MessageDto>()
                .ForMember(d => d.SenderName, o => o.MapFrom(s => $"{s.Sender.FirstName} {s.Sender.LastName}"))
                .ForMember(d => d.SenderProfilePicture, o => o.MapFrom(s => s.Sender.ProfilePictureUrl))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.IsMyMessage, o => o.Ignore());

            // Conversation mappings
            CreateMap<Conversation, ConversationDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.UnreadCount, o => o.Ignore())
                .ForMember(d => d.LastMessage, o => o.Ignore());

            // Participant mappings
            CreateMap<ConversationParticipant, ParticipantDto>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
                .ForMember(d => d.ProfilePictureUrl, o => o.MapFrom(s => s.User.ProfilePictureUrl))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.User.Status.ToString()));

            // Friendship mappings
            CreateMap<Friendship, FriendshipDto>();

            // Notification mappings
            CreateMap<Notification, NotificationDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
        }
    }
}
