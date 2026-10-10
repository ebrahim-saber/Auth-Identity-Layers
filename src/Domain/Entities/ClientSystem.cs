using System;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// ربط العميل بالأنظمة اللي اتعاقد عليها (الحته الماليه/الكونتراكت هتتضاف لاحقًا هنا).
    /// </summary>
    public class ClientSystem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = default!;

        public Guid SystemId { get; set; }
        public SystemEntity System { get; set; } = default!;

        public DateTime ContractedAt { get; set; } = DateTime.UtcNow;

        /// <summary>تاريخ بداية العقد (اختياري).</summary>
        public DateTime? ContractStartDate { get; set; }

        /// <summary>تاريخ نهاية العقد/الاشتراك (اختياري - يتم الفحص لمنع الوصول بعد انتهائه).</summary>
        public DateTime? ContractEndDate { get; set; }

        /// <summary>الحد الأقصى لعدد المستخدمين المسموح به لهذا النظام (null = غير محدود).</summary>
        public int? MaxUsersQuota { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
