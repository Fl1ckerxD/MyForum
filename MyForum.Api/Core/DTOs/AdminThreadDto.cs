namespace MyForum.Api.Core.DTOs
{
    public class AdminThreadDto
    {
        public int Id { get; init; }
        public string BoardShortName { get; init; }
        public string Title { get; init; }
        public int PostsCount { get; init; }

        public bool IsLocked { get; init; }
        public bool IsPinned { get; init; }
        public bool IsDeleted { get; init; }
        public DateTime? DeletedAt { get; init; }

        public DateTime CreatedAt { get; init; }
        public DateTime LastBumpAt { get; init; }
    }
}