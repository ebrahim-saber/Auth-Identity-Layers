using System;
using IdentityAuthLayer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using IdentityAuthLayer.Infrastructure.Persistence;

#nullable disable

namespace IdentityAuthLayer.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity<Client>(b =>
            {
                b.ToTable("Clients"); b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.ContactEmail).HasMaxLength(200); b.Property(x => x.ContactPhone).HasMaxLength(50);
                b.HasMany(x => x.ClientSystems).WithOne(x => x.Client).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(x => x.Users).WithOne(x => x.Client).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SystemEntity>(b =>
            {
                b.ToTable("Systems"); b.HasKey(x => x.Id); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
            });
            modelBuilder.Entity<ClientSystem>(b =>
            {
                b.ToTable("ClientSystems"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.ClientId, x.SystemId }).IsUnique();
                b.HasOne(x => x.Client).WithMany(x => x.ClientSystems).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.System).WithMany(x => x.ClientSystems).HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users"); b.HasKey(x => x.Id);
                b.Property(x => x.Username).HasMaxLength(100).IsRequired(); b.Property(x => x.FirstName).HasMaxLength(100).IsRequired(); b.Property(x => x.LastName).HasMaxLength(100).IsRequired(); b.Property(x => x.Email).HasMaxLength(200).IsRequired(); b.Property(x => x.Phone).HasMaxLength(30).IsRequired(); b.Property(x => x.PasswordHash); b.Property(x => x.UserType).HasMaxLength(20).HasConversion<string>().IsRequired(); b.Property(x => x.FailedLoginAttempts).IsRequired(); b.Property(x => x.LockoutEnd);
                b.HasIndex(x => x.Username).IsUnique(); b.HasIndex(x => x.Email).IsUnique();
                b.HasOne(x => x.Client).WithMany(x => x.Users).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
                b.OwnsOne(x => x.EmailVerification, ev => { ev.Property(p => p.Code).HasColumnName("EmailVerification_Code").HasMaxLength(64); ev.Property(p => p.CodeExpiry).HasColumnName("EmailVerification_CodeExpiry"); ev.Property(p => p.IsVerified).HasColumnName("EmailVerification_IsVerified"); });
                b.OwnsOne(x => x.PasswordReset, pr => { pr.Property(p => p.Code).HasColumnName("PasswordReset_Code").HasMaxLength(64); pr.Property(p => p.CodeExpiry).HasColumnName("PasswordReset_CodeExpiry"); pr.Property(p => p.IsVerified).HasColumnName("PasswordReset_IsVerified"); });
                b.OwnsOne(x => x.RefreshTokenInfo, rt => { rt.Property(p => p.RefreshToken).HasColumnName("RefreshToken").HasMaxLength(200); rt.Property(p => p.RefreshTokenExpiryTime).HasColumnName("RefreshTokenExpiryTime"); rt.Property(p => p.LastLoginDate).HasColumnName("LastLoginDate"); });
            });
            modelBuilder.Entity<Role>(b => { b.ToTable("Roles"); b.HasKey(x => x.Id); b.Property(x => x.Name).HasMaxLength(100).IsRequired(); b.HasIndex(x => new { x.SystemId, x.Name }).IsUnique(); b.HasOne(x => x.System).WithMany(x => x.Roles).HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict); });
            modelBuilder.Entity<Permission>(b => { b.ToTable("Permissions"); b.HasKey(x => x.Id); b.Property(x => x.Code).HasMaxLength(100).IsRequired(); b.Property(x => x.Name).HasMaxLength(150).IsRequired(); b.HasIndex(x => new { x.SystemId, x.Code }).IsUnique(); b.HasOne(x => x.System).WithMany(x => x.Permissions).HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict); });
            modelBuilder.Entity<RolePermission>(b => { b.ToTable("RolePermissions"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique(); b.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict); });
            modelBuilder.Entity<UserRole>(b => { b.ToTable("UserRoles"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique(); b.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict); });
#pragma warning restore 612, 618
        }
    }
}
