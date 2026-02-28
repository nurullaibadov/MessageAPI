using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
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
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
            => await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId && cp.IsActive)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Participants).ThenInclude(p => p.User)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Select(cp => cp.Conversation)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

        public async Task<Conversation?> GetConversationWithParticipantsAsync(Guid conversationId)
            => await _context.Conversations
                .Include(c => c.Participants).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

        public async Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2)
            => await _context.Conversations
                .Where(c => c.Type == ConversationType.Private)
                .Where(c => c.Participants.Any(p => p.UserId == userId1 && p.IsActive) &&
                            c.Participants.Any(p => p.UserId == userId2 && p.IsActive))
                .FirstOrDefaultAsync();

        public async Task<bool> IsUserParticipantAsync(Guid conversationId, Guid userId)
            => await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId && cp.IsActive);
    }
}
