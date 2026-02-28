using MessageAPI.Domain.Interfaces;
using MessageAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IUserRepository Users { get; }
        public IConversationRepository Conversations { get; }
        public IMessageRepository Messages { get; }
        public IFriendshipRepository Friendships { get; }
        public INotificationRepository Notifications { get; }

        public UnitOfWork(AppDbContext context, IUserRepository users,
            IConversationRepository conversations, IMessageRepository messages,
            IFriendshipRepository friendships, INotificationRepository notifications)
        {
            _context = context;
            Users = users;
            Conversations = conversations;
            Messages = messages;
            Friendships = friendships;
            Notifications = notifications;
        }

        // ← BU METODU EKLE — MessageService ve ConversationService kullanıyor
        public AppDbContext GetContext() => _context;

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _transaction!.CommitAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction!.RollbackAsync();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
