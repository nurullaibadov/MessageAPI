using MessageAPI.Domain.Entities;
using MessageAPI.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var adminSettings = serviceProvider.GetRequiredService<IOptions<AdminSettings>>().Value;

            // Seed roles
            string[] roles = { "SuperAdmin", "Admin", "Moderator", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new Role { Name = role, Description = $"{role} role" });
            }

            // Seed super admin
            var adminUser = await userManager.FindByEmailAsync(adminSettings.DefaultAdminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminSettings.DefaultAdminUsername,
                    Email = adminSettings.DefaultAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    IsEmailVerified = true,
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminSettings.DefaultAdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRolesAsync(adminUser, new[] { "SuperAdmin", "Admin", "User" });
            }
        }
    }
}
