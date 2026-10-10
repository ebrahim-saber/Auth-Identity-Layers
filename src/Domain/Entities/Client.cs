using System;
using System.Collections.Generic;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// العميل اللي بيتعاقد مع الشركة على واحد أو أكتر من الأنظمة (Systems).
    /// </summary>
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = default!;

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // الأنظمة اللي العميل متعاقد عليها
        public ICollection<ClientSystem> ClientSystems { get; set; } = new List<ClientSystem>();

        // اليوزرز التابعين للعميل ده
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
