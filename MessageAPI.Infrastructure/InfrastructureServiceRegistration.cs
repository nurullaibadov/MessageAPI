using MessageAPI.Application.Interfaces;
using MessageAPI.Domain.Entities;
using MessageAPI.Domain.Interfaces;
using MessageAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using MessageAPI.Infrastructure.Repositories;
using MessageAPI.Infrastructure.Services;
using MessageAPI.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("MessageAPI.Infrastructure")));

            // Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services — tam namespace ile Umbraco çakışmasını önle
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IUserService,
                MessageAPI.Infrastructure.Services.UserService>();
            services.AddScoped<IFriendshipService, FriendshipService>();
            services.AddScoped<INotificationService,
                MessageAPI.Infrastructure.Services.NotificationService>();
            services.AddScoped<IAdminService, AdminService>();

            // Settings
            services.Configure<JwtSettings>(o =>
                config.GetSection("JwtSettings").Bind(o));
            services.Configure<EmailSettings>(o =>
                config.GetSection("EmailSettings").Bind(o));
            services.Configure<AdminSettings>(o =>
                config.GetSection("AdminSettings").Bind(o));

            // SignalR
            services.AddSignalR();

            return services;
        }
    }
}
