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
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork uow, UserManager<User> userManager, IMapper mapper)
        {
            _uow = uow; _userManager = userManager; _mapper = mapper;
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted) return Result<UserDto>.NotFound("User not found.");
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Result<UserDto>.Success(dto);
        }

        public async Task<Result<UserDto>> GetUserByUsernameAsync(string username)
        {
            var user = await _uow.Users.GetByUsernameAsync(username);
            if (user == null) return Result<UserDto>.NotFound("User not found.");
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Result<UserDto>.Success(dto);
        }

        public async Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result<UserDto>.NotFound();

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.UserName)
            {
                if (await _uow.Users.UsernameExistsAsync(dto.Username))
                    return Result<UserDto>.Failure("Username already taken.");
                user.UserName = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (dto.Bio != null) user.Bio = dto.Bio;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            var result = _mapper.Map<UserDto>(user);
            result.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Result<UserDto>.Success(result);
        }

        public async Task<Result<IEnumerable<UserDto>>> SearchUsersAsync(string query, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Result<IEnumerable<UserDto>>.Failure("Search query must be at least 2 characters.");
            var users = await _uow.Users.SearchUsersAsync(query, currentUserId);
            return Result<IEnumerable<UserDto>>.Success(_mapper.Map<IEnumerable<UserDto>>(users));
        }

        public async Task<Result> UpdateStatusAsync(Guid userId, string status)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("Not found.", 404);
            if (Enum.TryParse<UserStatus>(status, out var userStatus)) user.Status = userStatus;
            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> DeactivateAccountAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("Not found.", 404);
            user.IsActive = false;
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }
    }
}
