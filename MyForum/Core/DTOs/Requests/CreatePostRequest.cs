namespace MyForum.Core.DTOs.Requests
{
    public record CreatePostRequest
    (
        int ThreadId,
        string Content,
        string AuthorName,
        string? PostPassword,
        List<IFormFile>? Files
    );
}