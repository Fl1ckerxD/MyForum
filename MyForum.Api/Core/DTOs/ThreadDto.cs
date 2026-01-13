namespace MyForum.Api.Core.DTOs
{
    public record ThreadDto
    {
        public int Id { get; init; }
        public string Subject { get; init; }
        public DateTime CreatedAt { get; init; }
        public PostDto OriginalPost { get; init; }
        public int PostCount { get; init; }
        public int FileCount { get; init; }
        public IEnumerable<PostDto> Posts { get; init; }
    }
}