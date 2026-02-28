using AutoMapper;
using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
using MessageAPI.Domain.Interfaces;
using MessageAPI.Infrastructure.Data;
using MessageAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ConversationService(IUnitOfWork uow, IMapper mapper, AppDbContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<ConversationDto>> GetOrCreatePrivateConversationAsync(Guid userId, Guid targetUserId)
        {
            var existing = await _uow.Conversations.GetPrivateConversationAsync(userId, targetUserId);
            if (existing != null)
                return Result<ConversationDto>.Success(await BuildConversationDto(existing, userId));

            var conversation = new Conversation
            {
                Type = ConversationType.Private,
                CreatedById = userId,
                Participants = new List<ConversationParticipant>
            {
                new() { UserId = userId, Role = ParticipantRole.Member },
                new() { UserId = targetUserId, Role = ParticipantRole.Member }
            }
            };

            await _uow.Conversations.AddAsync(conversation);
            await _uow.SaveChangesAsync();

            var fullConv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversation.Id);
            return Result<ConversationDto>.Success(await BuildConversationDto(fullConv!, userId), 201);
        }

        public async Task<Result<ConversationDto>> CreateGroupAsync(Guid creatorId, CreateGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result<ConversationDto>.Failure("Group name is required.");

            var participants = new List<ConversationParticipant>
        {
            new() { UserId = creatorId, Role = ParticipantRole.Owner }
        };
            participants.AddRange(dto.ParticipantIds.Where(id => id != creatorId)
                .Select(id => new ConversationParticipant { UserId = id, Role = ParticipantRole.Member }));

            var conversation = new Conversation
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = ConversationType.Group,
                CreatedById = creatorId,
                Participants = participants
            };

            await _uow.Conversations.AddAsync(conversation);
            await _uow.SaveChangesAsync();

            var fullConv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversation.Id);
            return Result<ConversationDto>.Success(await BuildConversationDto(fullConv!, creatorId), 201);
        }

        public async Task<Result<IEnumerable<ConversationDto>>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _uow.Conversations.GetUserConversationsAsync(userId);
            var dtos = new List<ConversationDto>();
            foreach (var conv in conversations)
                dtos.Add(await BuildConversationDto(conv, userId));
            return Result<IEnumerable<ConversationDto>>.Success(dtos);
        }

        public async Task<Result<ConversationDto>> GetConversationAsync(Guid conversationId, Guid userId)
        {
            var isParticipant = await _uow.Conversations.IsUserParticipantAsync(conversationId, userId);
            if (!isParticipant) return Result<ConversationDto>.Forbidden("Not a participant.");

            var conv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversationId);
            if (conv == null) return Result<ConversationDto>.NotFound();
            return Result<ConversationDto>.Success(await BuildConversationDto(conv, userId));
        }

        public async Task<Result> AddParticipantAsync(Guid conversationId, Guid requesterId, Guid newUserId)
        {
            var conv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversationId);
            if (conv == null) return Result.Failure("Conversation not found.", 404);
            if (conv.Type != ConversationType.Group) return Result.Failure("Can only add participants to group conversations.");

            var requesterParticipant = conv.Participants.FirstOrDefault(p => p.UserId == requesterId && p.IsActive);
            if (requesterParticipant == null || requesterParticipant.Role == ParticipantRole.Member)
                return Result.Failure("Only admins can add participants.", 403);

            if (conv.Participants.Any(p => p.UserId == newUserId && p.IsActive))
                return Result.Failure("User is already a participant.");

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = newUserId,
                Role = ParticipantRole.Member
            });
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> RemoveParticipantAsync(Guid conversationId, Guid requesterId, Guid userId)
        {
            var conv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversationId);
            if (conv == null) return Result.Failure("Not found.", 404);

            var requester = conv.Participants.FirstOrDefault(p => p.UserId == requesterId && p.IsActive);
            if (requester == null || requester.Role == ParticipantRole.Member)
                return Result.Failure("Not authorized.", 403);

            var target = conv.Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);
            if (target == null) return Result.Failure("User not found in conversation.");
            if (target.Role == ParticipantRole.Owner) return Result.Failure("Cannot remove the owner.");

            target.IsActive = false;
            target.LeftAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> LeaveConversationAsync(Guid conversationId, Guid userId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId && cp.IsActive);
            if (participant == null) return Result.Failure("Not a participant.");

            participant.IsActive = false;
            participant.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateGroupAsync(Guid conversationId, Guid userId, UpdateGroupDto dto)
        {
            var conv = await _uow.Conversations.GetConversationWithParticipantsAsync(conversationId);
            if (conv == null) return Result.Failure("Not found.", 404);

            var participant = conv.Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);
            if (participant == null || participant.Role == ParticipantRole.Member)
                return Result.Failure("Not authorized.", 403);

            if (!string.IsNullOrEmpty(dto.Name)) conv.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) conv.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.ImageUrl)) conv.ImageUrl = dto.ImageUrl;
            conv.UpdatedAt = DateTime.UtcNow;
            await _uow.Conversations.UpdateAsync(conv);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            var conv = await _uow.Conversations.GetByIdAsync(conversationId);
            if (conv == null) return Result.Failure("Not found.", 404);
            if (conv.CreatedById != userId) return Result.Failure("Not authorized.", 403);

            conv.IsDeleted = true;
            conv.DeletedAt = DateTime.UtcNow;
            await _uow.Conversations.UpdateAsync(conv);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        private async Task<ConversationDto> BuildConversationDto(Conversation conv, Guid userId)
        {
            var dto = _mapper.Map<ConversationDto>(conv);
            dto.Participants = conv.Participants?
                .Where(p => p.IsActive)
                .Select(p => _mapper.Map<ParticipantDto>(p)).ToList() ?? new();
            dto.UnreadCount = await _uow.Messages.GetUnreadCountAsync(conv.Id, userId);
            var lastMsg = conv.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            if (lastMsg != null) dto.LastMessage = _mapper.Map<MessageDto>(lastMsg);
            return dto;
        }
    }
}
