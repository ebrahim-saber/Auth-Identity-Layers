using System;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// ربط اليوزر بـ Role معين. بما إن الـ Role نفسه مربوط بسيستم واحد،
    /// فده تلقائيًا بيحدد "اليوزر ده عنده الرول ده على السيستم ده".
    /// نفس اليوزر ممكن ياخد نفس اسم الرول أو صلاحية زي "ViewReports" على أكتر من سيستم
    /// لأن كل سجل UserRole بيشاور على Role مستقل (Role.SystemId مختلف).
    /// </summary>
    public class UserRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Domain.Entities.User User { get; set; } = default!;

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
