using AutoMapper;
using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
using MessageAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public FriendshipService(IUnitOfWork uow, IMapper mapper, UserManager<User> userManager)
        {
            _uow = uow; _mapper = mapper; _userManager = userManager;
        }

        public async Task<Result> SendFriendRequestAsync(Guid requesterId, Guid addresseeId)
        {
            if (requesterId == addresseeId) return Result.Failure("Cannot send friend request to yourself.");

            var existing = await _uow.Friendships.GetFriendshipAsync(requesterId, addresseeId);
            if (existing != null)
            {
                return existing.Status switch
                {
                    FriendshipStatus.Pending => Result.Failure("Friend request already sent."),
                    FriendshipStatus.Accepted => Result.Failure("Already friends."),
                    FriendshipStatus.Blocked => Result.Failure("Cannot send request."),
                    _ => Result.Failure("Request already exists.")
                };
            }

            await _uow.Friendships.AddAsync(new Friendship
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = FriendshipStatus.Pending
            });
            await _uow.SaveChangesAsync();
            return Result.Success(201);
        }

        public async Task<Result> AcceptFriendRequestAsync(Guid userId, Guid requesterId)
        {
            var friendship = await _uow.Friendships.GetFriendshipAsync(requesterId, userId);
            if (friendship == null || friendship.Status != FriendshipStatus.Pending)
                return Result.Failure("No pending friend request found.");

            friendship.Status = FriendshipStatus.Accepted;
            friendship.RespondedAt = DateTime.UtcNow;
            await _uow.Friendships.UpdateAsync(friendship);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeclineFriendRequestAsync(Guid userId, Guid requesterId)
        {
            var friendship = await _uow.Friendships.GetFriendshipAsync(requesterId, userId);
            if (friendship == null) return Result.Failure("Request not found.", 404);
            friendship.Status = FriendshipStatus.Declined;
            friendship.RespondedAt = DateTime.UtcNow;
            await _uow.Friendships.UpdateAsync(friendship);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> RemoveFriendAsync(Guid userId, Guid friendId)
        {
            var friendship = await _uow.Friendships.GetFriendshipAsync(userId, friendId)
                ?? await _uow.Friendships.GetFriendshipAsync(friendId, userId);
            if (friendship == null) return Result.Failure("Not friends.", 404);
            friendship.IsDeleted = true;
            await _uow.Friendships.UpdateAsync(friendship);
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> BlockUserAsync(Guid userId, Guid targetId)
        {
            var friendship = await _uow.Friendships.GetFriendshipAsync(userId, targetId)
                ?? await _uow.Friendships.GetFriendshipAsync(targetId, userId);

            if (friendship != null)
            {
                friendship.Status = FriendshipStatus.Blocked;
                friendship.RequesterId = userId;
                friendship.AddresseeId = targetId;
                await _uow.Friendships.UpdateAsync(friendship);
            }
            else
            {
                await _uow.Friendships.AddAsync(new Friendship
                {
                    RequesterId = userId,
                    AddresseeId = targetId,
                    Status = FriendshipStatus.Blocked
                });
            }
            await _uow.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IEnumerable<UserDto>>> GetFriendsAsync(Guid userId)
        {
            var friendships = await _uow.Friendships.GetUserFriendsAsync(userId);
            var friends = friendships.Select(f => f.RequesterId == userId ? f.Addressee : f.Requester);
            return Result<IEnumerable<UserDto>>.Success(_mapper.Map<IEnumerable<UserDto>>(friends));
        }

        public async Task<Result<IEnumerable<UserDto>>> GetPendingRequestsAsync(Guid userId)
        {
            var requests = await _uow.Friendships.GetPendingRequestsAsync(userId);
            var requesters = requests.Select(f => f.Requester);
            return Result<IEnumerable<UserDto>>.Success(_mapper.Map<IEnumerable<UserDto>>(requesters));
        }
    }
}
