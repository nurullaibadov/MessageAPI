using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Interfaces;
using MessageAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int page, int pageSize)
            => await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .Include(m => m.ReplyToMessage).ThenInclude(r => r!.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (participant == null) return 0;
            var lastRead = participant.LastReadAt ?? DateTime.MinValue;
            return await _context.Messages
                .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && m.CreatedAt > lastRead);
        }

        public async Task MarkAsReadAsync(Guid conversationId, Guid userId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (participant != null)
            {
                participant.LastReadAt = DateTime.UtcNow;
                _context.ConversationParticipants.Update(participant);
            }
        }

        public async Task<Message?> GetMessageWithDetailsAsync(Guid messageId)
            => await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .Include(m => m.ReplyToMessage).ThenInclude(r => r!.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);
    }
}
