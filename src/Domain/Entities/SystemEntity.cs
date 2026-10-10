using System;
using System.Collections.Generic;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// نظام من أنظمة الشركة (مثال: تراكينج = 1، إدارة مشروع الحج = 2، مشروع المعاقين = 3).
    /// كل Role وكل Permission لازم يكونوا مرتبطين بسيستم موجود هنا.
    /// </summary>
    public class SystemEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>اسم مميز للسيستم (Code) يستخدم في الاتصال بين الـ APIs.</summary>
        public string Code { get; set; } = default!; // e.g. "TRACKING", "HAJJ", "DISABILITY"

        public string Name { get; set; } = default!; // اسم وصفي

        public string? Description { get; set; }

        /// <summary>مفتاح سري للتواصل الآلي المباشر (Machine-to-Machine) بين هذا السيستم الفرعي والـ API المركزية.</summary>
        public string ApiKey { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ClientSystem> ClientSystems { get; set; } = new List<ClientSystem>();

        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
