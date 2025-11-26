namespace MyForum.Web.Requests
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