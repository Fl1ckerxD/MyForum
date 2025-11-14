
using System.ComponentModel.DataAnnotations;

namespace MyForum.Core.Entities
{
    public class Board
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string ShortName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int Position { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public ICollection<Thread> Threads { get; set; } = new List<Thread>();
        public ICollection<BoardModerator> Moderators { get; set; } = new List<BoardModerator>();
    }
}