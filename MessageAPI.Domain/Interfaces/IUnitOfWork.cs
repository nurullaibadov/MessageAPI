using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        IFriendshipRepository Friendships { get; }
        INotificationRepository Notifications { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        // Domain'den AppDbContext'e bağımlılık olmaması için object döndürüyoruz
        // Infrastructure içinde cast edeceğiz
    }
}
