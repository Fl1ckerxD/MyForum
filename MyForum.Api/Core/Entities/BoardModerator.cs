using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.Entities
{
    public class BoardModerator
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool CanDeletePosts { get; set; } = true;
        public bool CanBanUsers { get; set; } = true;
        public bool CanManageThreads { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Внешние ключи
        public int BoardId { get; set; }

        // Навигационные свойства
        public Board Board { get; set; }
    }
}