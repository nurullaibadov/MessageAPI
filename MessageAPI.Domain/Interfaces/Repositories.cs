using MessageAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        IQueryable<T> GetQueryable();
    }

    // Interfaces/IUserRepository.cs
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<IEnumerable<User>> SearchUsersAsync(string query, Guid excludeUserId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }

    // Interfaces/IConversationRepository.cs
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
        Task<Conversation?> GetConversationWithParticipantsAsync(Guid conversationId);
        Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2);
        Task<bool> IsUserParticipantAsync(Guid conversationId, Guid userId);
    }

    // Interfaces/IMessageRepository.cs
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);
        Task MarkAsReadAsync(Guid conversationId, Guid userId);
        Task<Message?> GetMessageWithDetailsAsync(Guid messageId);
    }

    // Interfaces/IFriendshipRepository.cs
    public interface IFriendshipRepository : IGenericRepository<Friendship>
    {
        Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2);
        Task<IEnumerable<Friendship>> GetUserFriendsAsync(Guid userId);
        Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId);
    }

    // Interfaces/INotificationRepository.cs
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
    }


}
