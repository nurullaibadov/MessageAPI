using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Entities
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }

    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
