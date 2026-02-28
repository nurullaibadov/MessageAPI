using AutoMapper;
using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
using MessageAPI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<NotificationDto>>> GetNotificationsAsync(
            Guid userId, bool unreadOnly = false)
        {
            var items = await _uow.Notifications.GetUserNotificationsAsync(userId, unreadOnly);
            return Result<IEnumerable<NotificationDto>>.Success(
                _mapper.Map<IEnumerable<NotificationDto>>(items));
        }

        public async Task<Result<int>> GetUnreadCountAsync(Guid userId)
        {
            var count = await _uow.Notifications.GetUnreadCountAsync(userId);
            return Result<int>.Success(count);
        }

        public async Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var n = await _uow.Notifications.GetByIdAsync(notificationId);
            if (n == null || n.UserId != userId)
                return Result.Failure("Notification not found.", 404);
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _uow.Notifications.UpdateAsync(n);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MarkAllAsReadAsync(Guid userId)
        {
            await _uow.Notifications.MarkAllAsReadAsync(userId);
            return Result.Success();
        }

        public async Task SendNotificationAsync(Guid userId, string title, string content,
            NotificationType type, string? referenceId = null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceId = referenceId
            });
            await _uow.SaveChangesAsync();
        }
    }
}
