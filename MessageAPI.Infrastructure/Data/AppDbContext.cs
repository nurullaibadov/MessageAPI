using MessageAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid,
      IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
      IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<MessageReadStatus> MessageReadStatuses { get; set; }
        public DbSet<MessageReaction> MessageReactions { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Rename Identity tables
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Soft delete query filter
            builder.Entity<Message>().HasQueryFilter(m => !m.IsDeleted);
            builder.Entity<Conversation>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<ConversationParticipant>().HasQueryFilter(cp => !cp.IsDeleted);
            builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
            builder.Entity<Friendship>().HasQueryFilter(f => !f.IsDeleted);

            // UserRole relationships
            builder.Entity<UserRole>()
                .HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            builder.Entity<UserRole>()
                .HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);

            // Message relationships
            builder.Entity<Message>()
                .HasOne(m => m.Sender).WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.ReplyToMessage).WithMany()
                .HasForeignKey(m => m.ReplyToMessageId).OnDelete(DeleteBehavior.Restrict);

            // Conversation -> CreatedBy
            builder.Entity<Conversation>()
                .HasOne(c => c.CreatedBy).WithMany()
                .HasForeignKey(c => c.CreatedById).OnDelete(DeleteBehavior.SetNull);

            // Friendship
            builder.Entity<Friendship>()
                .HasOne(f => f.Requester).WithMany(u => u.SentFriendRequests)
                .HasForeignKey(f => f.RequesterId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Friendship>()
                .HasOne(f => f.Addressee).WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(f => f.AddresseeId).OnDelete(DeleteBehavior.Restrict);

            // MessageReaction unique constraint
            builder.Entity<MessageReaction>()
                .HasIndex(r => new { r.MessageId, r.UserId, r.Emoji }).IsUnique();

            // Indexes
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            builder.Entity<Message>().HasIndex(m => m.ConversationId);
            builder.Entity<Message>().HasIndex(m => m.SenderId);
            builder.Entity<ConversationParticipant>().HasIndex(cp => new { cp.ConversationId, cp.UserId }).IsUnique();
        }
    }
}
