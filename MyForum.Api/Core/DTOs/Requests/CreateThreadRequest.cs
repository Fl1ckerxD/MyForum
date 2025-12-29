namespace MyForum.Api.Core.DTOs.Requests
{
    public record CreateThreadRequest
    (
        int BoardId,
        string BoardShortName,
        string Subject,
        CreatePostRequest OriginalPost
    );
}