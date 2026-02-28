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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted);

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
            => await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        public async Task<IEnumerable<User>> SearchUsersAsync(string query, Guid excludeUserId)
            => await _context.Users
                .Where(u => u.Id != excludeUserId && !u.IsDeleted && u.IsActive &&
                    (u.UserName!.Contains(query) || u.FirstName.Contains(query) ||
                     u.LastName.Contains(query) || u.Email!.Contains(query)))
                .Take(20).ToListAsync();

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> UsernameExistsAsync(string username)
            => await _context.Users.AnyAsync(u => u.UserName == username);
    }
}
