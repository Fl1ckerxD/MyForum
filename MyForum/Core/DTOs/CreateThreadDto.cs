namespace MyForum.Core.DTOs
{
    public record CreateThreadDto
    (
        int BoardId,
        string BoardShortName,
        string Subject,
        CreatePostDto OriginalPost
    );
}