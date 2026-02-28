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
    public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
    {
        public FriendshipRepository(AppDbContext context) : base(context) { }

        public async Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2)
            => await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId1 && f.AddresseeId == userId2) ||
                    (f.RequesterId == userId2 && f.AddresseeId == userId1));

        public async Task<IEnumerable<Friendship>> GetUserFriendsAsync(Guid userId)
            => await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                    (f.RequesterId == userId || f.AddresseeId == userId))
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .ToListAsync();

        public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId)
            => await _context.Friendships
                .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
                .Include(f => f.Requester)
                .ToListAsync();
    }
}
