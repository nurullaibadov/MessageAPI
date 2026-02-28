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
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public MessageService(IUnitOfWork uow, IMapper mapper, AppDbContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<MessageDto>> SendMessageAsync(Guid senderId, SendMessageDto dto)
        {
            var isParticipant = await _uow.Conversations.IsUserParticipantAsync(dto.ConversationId, senderId);
            if (!isParticipant)
                return Result<MessageDto>.Forbidden("You are not a participant of this conversation.");

            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = senderId,
                Content = dto.Content,
                Type = Enum.TryParse<MessageType>(dto.Type, out var mType) ? mType : MessageType.Text,
                ReplyToMessageId = dto.ReplyToMessageId,
                Status = MessageStatus.Sent
            };

            await _uow.Messages.AddAsync(message);

            var conversation = await _uow.Conversations.GetByIdAsync(dto.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.UtcNow;
                await _uow.Conversations.UpdateAsync(conversation);
            }

            await _uow.SaveChangesAsync();

            var savedMessage = await _uow.Messages.GetMessageWithDetailsAsync(message.Id);
            var messageDto = _mapper.Map<MessageDto>(savedMessage);
            messageDto.IsMyMessage = true;
            return Result<MessageDto>.Success(messageDto, 201);
        }

        public async Task<Result<PagedResult<MessageDto>>> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize)
        {
            var isParticipant = await _uow.Conversations.IsUserParticipantAsync(conversationId, userId);
            if (!isParticipant)
                return Result<PagedResult<MessageDto>>.Forbidden("Not a participant.");

            var messages = (await _uow.Messages.GetConversationMessagesAsync(conversationId, page, pageSize)).ToList();
            var dtos = _mapper.Map<List<MessageDto>>(messages);
            dtos.ForEach(m => m.IsMyMessage = m.SenderId == userId);

            var totalCount = await _context.Messages.CountAsync(m => m.ConversationId == conversationId);

            return Result<PagedResult<MessageDto>>.Success(new PagedResult<MessageDto>
            {
                Items = dtos.AsEnumerable().Reverse(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<MessageDto>> EditMessageAsync(Guid messageId, Guid userId, EditMessageDto dto)
        {
            var message = await _uow.Messages.GetMessageWithDetailsAsync(messageId);
            if (message == null) return Result<MessageDto>.NotFound("Message not found.");
            if (message.SenderId != userId) return Result<MessageDto>.Forbidden("You can only edit your own messages.");

            message.Content = dto.Content;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;
            await _uow.Messages.UpdateAsync(message);
            await _uow.SaveChangesAsync();

            var messageDto = _mapper.Map<MessageDto>(message);
            messageDto.IsMyMessage = true;
            return Result<MessageDto>.Success(messageDto);
        }

        public async Task<Result> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _uow.Messages.GetByIdAsync(messageId);
            if (message == null) return Result.Failure("Message not found.", 404);
            if (message.SenderId != userId) return Result.Failure("Not authorized.", 403);

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            message.Content = "[Message deleted]";
            await _uow.Messages.UpdateAsync(message);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MarkAsReadAsync(Guid conversationId, Guid userId)
        {
            await _uow.Messages.MarkAsReadAsync(conversationId, userId);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<MessageDto>> AddReactionAsync(Guid messageId, Guid userId, string emoji)
        {
            var message = await _uow.Messages.GetByIdAsync(messageId);
            if (message == null) return Result<MessageDto>.NotFound("Message not found.");

            var existing = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (existing == null)
            {
                _context.MessageReactions.Add(new MessageReaction { MessageId = messageId, UserId = userId, Emoji = emoji });
                await _context.SaveChangesAsync();
            }

            var updated = await _uow.Messages.GetMessageWithDetailsAsync(messageId);
            return Result<MessageDto>.Success(_mapper.Map<MessageDto>(updated!));
        }

        public async Task<Result> RemoveReactionAsync(Guid messageId, Guid userId, string emoji)
        {
            var reaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);
            if (reaction != null)
            {
                _context.MessageReactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
            return Result.Success();
        }
    }
}
