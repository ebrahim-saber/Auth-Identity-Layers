using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ContactEmail).HasMaxLength(200);
            builder.Property(x => x.ContactPhone).HasMaxLength(50);

            builder.HasMany(x => x.ClientSystems)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Users)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SystemEntityConfiguration : IEntityTypeConfiguration<SystemEntity>
    {
        public void Configure(EntityTypeBuilder<SystemEntity> builder)
        {
            builder.ToTable("Systems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ApiKey).IsRequired().HasMaxLength(128);
            builder.HasIndex(x => x.ApiKey).IsUnique();
        }
    }

    public class ClientSystemConfiguration : IEntityTypeConfiguration<ClientSystem>
    {
        public void Configure(EntityTypeBuilder<ClientSystem> builder)
        {
            builder.ToTable("ClientSystems");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.System)
                .WithMany(x => x.ClientSystems)
                .HasForeignKey(x => x.SystemId)
                .OnDelete(DeleteBehavior.Restrict);

            // العميل ميتكررش على نفس السيستم مرتين
            builder.HasIndex(x => new { x.ClientId, x.SystemId }).IsUnique();
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Username).IsRequired().HasMaxLength(100);
            builder.HasIndex(x => x.Username).IsUnique();
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
            builder.HasIndex(x => x.Email).IsUnique();
            builder.Property(x => x.Phone).HasMaxLength(30);
            builder.Property(x => x.PasswordHash).IsRequired(false);
            builder.Property(x => x.UserType).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.FailedLoginAttempts).IsRequired();
            builder.Property(x => x.LockoutEnd);

            builder.OwnsOne(x => x.EmailVerification, ev =>
            {
                ev.Property(p => p.Code).HasColumnName("EmailVerification_Code").HasMaxLength(64);
                ev.Property(p => p.PreviousCode).HasColumnName("EmailVerification_PreviousCode").HasMaxLength(64);
                ev.Property(p => p.CodeExpiry).HasColumnName("EmailVerification_CodeExpiry");
                ev.Property(p => p.LastSentAt).HasColumnName("EmailVerification_LastSentAt");
                ev.Property(p => p.IsVerified).HasColumnName("EmailVerification_IsVerified");
            });

            builder.OwnsOne(x => x.PasswordReset, pr =>
            {
                pr.Property(p => p.Code).HasColumnName("PasswordReset_Code").HasMaxLength(64);
                pr.Property(p => p.PreviousCode).HasColumnName("PasswordReset_PreviousCode").HasMaxLength(64);
                pr.Property(p => p.CodeExpiry).HasColumnName("PasswordReset_CodeExpiry");
                pr.Property(p => p.LastSentAt).HasColumnName("PasswordReset_LastSentAt");
                pr.Property(p => p.IsVerified).HasColumnName("PasswordReset_IsVerified");
            });

            builder.OwnsOne(x => x.RefreshTokenInfo, rt =>
            {
                rt.Property(p => p.RefreshToken).HasColumnName("RefreshToken").HasMaxLength(200);
                rt.Property(p => p.RefreshTokenExpiryTime).HasColumnName("RefreshTokenExpiryTime");
                rt.Property(p => p.LastLoginDate).HasColumnName("LastLoginDate");
            });

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(x => x.System)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.SystemId)
                .OnDelete(DeleteBehavior.Restrict);

            // نفس اسم الرول ميتكررش على نفس السيستم
            builder.HasIndex(x => new { x.SystemId, x.Name }).IsUnique();

            builder.HasMany(x => x.RolePermissions)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);

            builder.HasOne(x => x.System)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.SystemId)
                .OnDelete(DeleteBehavior.Restrict);

            // نفس كود الصلاحية ميتكررش على نفس السيستم (بس ممكن يتكرر عبر أنظمة مختلفة كسجلات منفصلة)
            builder.HasIndex(x => new { x.SystemId, x.Code }).IsUnique();
        }
    }

    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        }
    }
}
