using System;
using System.Collections.Generic;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// Role مربوط دايمًا بسيستم واحد (SystemId). نفس اسم الـ Role ممكن يتكرر
    /// على أكتر من سيستم لكن كل نسخة سجل مستقل ليها Permissions خاصة بيها.
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SystemId { get; set; }
        public SystemEntity System { get; set; } = default!;

        public string Name { get; set; } = default!; // e.g. "Admin", "Viewer"

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
