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
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            Guid userId, bool unreadOnly = false)
        {
            var query = _context.Notifications.Where(n => n.UserId == userId);
            if (unreadOnly) query = query.Where(n => !n.IsRead);
            return await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            unread.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
            await _context.SaveChangesAsync();
        }
    }
}
