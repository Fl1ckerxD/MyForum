namespace MyForum.Core.DTOs
{
    public record CreatePostDto
    (
        int ThreadId,
        string Content,
        string AuthorName,
        string? PostPassword
    );
}