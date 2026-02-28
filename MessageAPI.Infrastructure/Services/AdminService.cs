using AutoMapper;
using MessageAPI.Application.DTOs;
using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Common;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Enums;
using MessageAPI.Domain.Interfaces;
using MessageAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AdminService(IUnitOfWork uow, UserManager<User> userManager, IMapper mapper, AppDbContext context)
        {
            _uow = uow; _userManager = userManager; _mapper = mapper; _context = context;
        }

        public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var stats = new DashboardStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
                ActiveUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive),
                OnlineUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Online),
                TotalMessages = await _context.Messages.IgnoreQueryFilters().CountAsync(m => !m.IsDeleted),
                TotalConversations = await _context.Conversations.IgnoreQueryFilters().CountAsync(c => !c.IsDeleted),
                TodayMessages = await _context.Messages.CountAsync(m => m.CreatedAt >= today),
                TodayNewUsers = await _context.Users.CountAsync(u => u.CreatedAt >= today),
            };

            stats.Last7DaysStats = await GetLast7DaysStatsAsync();
            return Result<DashboardStatsDto>.Success(stats);
        }

        private async Task<List<DailyStatDto>> GetLast7DaysStatsAsync()
        {
            var result = new List<DailyStatDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                result.Add(new DailyStatDto
                {
                    Date = date,
                    Messages = await _context.Messages.CountAsync(m => m.CreatedAt.Date == date),
                    NewUsers = await _context.Users.CountAsync(u => u.CreatedAt.Date == date)
                });
            }
            return result;
        }

        public async Task<Result<PagedResult<AdminUserDto>>> GetAllUsersAsync(int page, int pageSize, string? search)
        {
            var query = _context.Users.Where(u => !u.IsDeleted).AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.UserName!.Contains(search) || u.Email!.Contains(search) ||
                    u.FirstName.Contains(search) || u.LastName.Contains(search));

            var total = await query.CountAsync();
            var users = await query.OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = new List<AdminUserDto>();
            foreach (var user in users)
            {
                var dto = _mapper.Map<AdminUserDto>(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                dtos.Add(dto);
            }

            return Result<PagedResult<AdminUserDto>>.Success(new PagedResult<AdminUserDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<AdminUserDto>> GetUserDetailsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result<AdminUserDto>.NotFound();
            var dto = _mapper.Map<AdminUserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            dto.MessageCount = await _context.Messages.CountAsync(m => m.SenderId == userId);
            return Result<AdminUserDto>.Success(dto);
        }

        public async Task<Result> BanUserAsync(Guid userId, string reason)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);
            user.IsActive = false;
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return Result.Success();
        }

        public async Task<Result> UnbanUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);
            return Result.Success();
        }

        public async Task<Result> AssignRoleAsync(Guid userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);
            if (await _userManager.IsInRoleAsync(user, role)) return Result.Failure("User already has this role.");
            await _userManager.AddToRoleAsync(user, role);
            return Result.Success();
        }

        public async Task<Result> RemoveRoleAsync(Guid userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);
            await _userManager.RemoveFromRoleAsync(user, role);
            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Failure("User not found.", 404);
            user.IsDeleted = true;
            user.IsActive = false;
            user.RefreshToken = null;
            user.Email = $"deleted_{userId}@deleted.com";
            user.UserName = $"deleted_{userId}";
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<ConversationDto>>> GetAllConversationsAsync(int page, int pageSize)
        {
            var convs = await _context.Conversations
                .IgnoreQueryFilters()
                .Include(c => c.Participants).ThenInclude(p => p.User)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            return Result<IEnumerable<ConversationDto>>.Success(_mapper.Map<IEnumerable<ConversationDto>>(convs));
        }
    }
}
