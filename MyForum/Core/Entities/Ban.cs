using System.ComponentModel.DataAnnotations;

namespace MyForum.Core.Entities
{
    public class Ban
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(1000)]
        public string Reason { get; set; }

        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Внешние ключи
        public int? BoardId { get; set; } // null = глобальный бан

        // Навигационные свойства
        public Board? Board { get; set; }
    }
}