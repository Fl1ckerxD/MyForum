namespace MyForum.Api.Core.DTOs.Requests
{
    public record CreatePostRequest
    (
        int ThreadId,
        string Content,
        string AuthorName,
        List<IFormFile>? Files,
        int? ReplyToPostId
    );
}