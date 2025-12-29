using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string AuthorName { get; set; } = "Аноним";

        [MaxLength(50)]
        public string? AuthorTripcode { get; set; }

        [MaxLength(50)]
        public string? AuthorId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        [MaxLength(20)]
        public string? PostPassword { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Внешние ключи
        public int ThreadId { get; set; }
        public int? ReplyToPostId { get; set; }

        // Навигационные свойства
        public Thread Thread { get; set; }
        public Post? ReplyToPost { get; set; }
        public ICollection<PostFile> Files { get; set; } = new List<PostFile>();
        public ICollection<Post> Replies { get; set; } = new List<Post>();
    }
}