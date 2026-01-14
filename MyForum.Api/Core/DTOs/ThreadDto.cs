namespace MyForum.Api.Core.DTOs
{
    public record ThreadDto
    (
        int Id,
        string Subject,
        DateTime CreatedAt,
        PostDto OriginalPost,
        int PostCount,
        int FileCount,
        IEnumerable<PostDto> Posts
    );
}