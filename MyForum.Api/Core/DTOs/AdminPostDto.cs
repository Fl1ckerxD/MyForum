namespace MyForum.Api.Core.DTOs
{
    public class AdminPostDto
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public bool IsOriginal { get; set; }

        public string Author { get; set; } = null!;
        public string Content { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}