using System;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// ربط Role بـ Permission. لازم Role.SystemId == Permission.SystemId
    /// (بيتفحص في الـ Handler وقت الإضافة عشان منخليش رول ياخد صلاحية من سيستم تاني).
    /// </summary>
    public class RolePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;
    }
}
