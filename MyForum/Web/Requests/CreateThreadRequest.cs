namespace MyForum.Web.Requests
{
    public record CreateThreadRequest
    (
        int BoardId,
        string BoardShortName,
        string Subject,
        CreatePostRequest OriginalPost
    );
}