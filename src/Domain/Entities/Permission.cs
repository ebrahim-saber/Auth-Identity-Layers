using System;
using System.Collections.Generic;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// Permission مربوطة دايمًا بسيستم واحد (SystemId).
    /// مثال: "ViewReports" ممكن يكون ليها 3 سجلات مستقلة: واحدة للتراكينج، واحدة للحج، واحدة للمعاقين.
    /// </summary>
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SystemId { get; set; }
        public SystemEntity System { get; set; } = default!;

        /// <summary>كود يستخدم في السيرفر التاني للتحقق برمجيًا. مثال: "REPORTS_VIEW"</summary>
        public string Code { get; set; } = default!;

        public string Name { get; set; } = default!; // e.g. "View Reports"

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
