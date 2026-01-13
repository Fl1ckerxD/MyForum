using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.Entities
{
    public class Thread
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Subject { get; set; } = null!;
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastBumpAt { get; set; } = DateTime.UtcNow;
        public int PostCount { get; set; }
        public int FileCount { get; set; }
        public int ReplyCount { get; set; }

        // Внешние ключи
        public int BoardId { get; set; }

        // Навигационные свойства
        public Board Board { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}