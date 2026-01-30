namespace MyForum.Api.Core.DTOs
{
    public record ThreadDto
    (
        int Id,
        string Subject,
        DateTime CreatedAt,
        DateTime LastBumpAt,
        PostDto OriginalPost,
        int PostCount,
        int FileCount,
        BoardSummary? Board,
        IEnumerable<PostDto>? Posts
    );
}