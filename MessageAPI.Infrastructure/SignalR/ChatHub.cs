using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.SignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly IUserService _userService;
        private static readonly Dictionary<string, string> _userConnections = new(); // userId -> connectionId

        public ChatHub(IMessageService messageService, IConversationService conversationService, IUserService userService)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _userConnections[userId] = Context.ConnectionId;
                await _userService.UpdateStatusAsync(Guid.Parse(userId), "Online");
                await Clients.Others.SendAsync("UserOnline", userId);

                // Kullanıcının konuşmalarına subscribe et
                var conversations = await _conversationService.GetUserConversationsAsync(Guid.Parse(userId));
                if (conversations.IsSuccess && conversations.Data != null)
                {
                    foreach (var conv in conversations.Data)
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conv.Id}");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _userConnections.Remove(userId);
                await _userService.UpdateStatusAsync(Guid.Parse(userId), "Offline");
                await Clients.Others.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto dto)
        {
            var userId = Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _messageService.SendMessageAsync(userId, dto);
            if (result.IsSuccess)
            {
                await Clients.Group($"conversation_{dto.ConversationId}")
                    .SendAsync("ReceiveMessage", result.Data);
            }
        }

        public async Task JoinConversation(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task TypingStarted(Guid conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", userId, conversationId);
        }

        public async Task TypingStopped(Guid conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserStoppedTyping", userId, conversationId);
        }

        public async Task MarkAsRead(Guid conversationId)
        {
            var userId = Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _messageService.MarkAsReadAsync(conversationId, userId);
            await Clients.Group($"conversation_{conversationId}").SendAsync("MessagesRead", userId, conversationId);
        }
    }
}
