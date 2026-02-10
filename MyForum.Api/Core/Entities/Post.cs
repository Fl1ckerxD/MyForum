using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string AuthorName { get; set; } = "Аноним";

        [MaxLength(1500)]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(44)]
        public string IpAddressHash { get; set; } = null!;

        [MaxLength(256)]
        public string? UserAgent { get; set; }
        public bool IsOriginal { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Внешние ключи
        public int ThreadId { get; set; }
        public int? ReplyToPostId { get; set; }

        // Навигационные свойства
        public Thread Thread { get; set; } = null!;
        public Post? ReplyToPost { get; set; }
        public ICollection<PostFile> Files { get; set; } = new List<PostFile>();
        public ICollection<Post> Replies { get; set; } = new List<Post>();
    }
}