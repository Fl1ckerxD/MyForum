namespace MyForum.Web.Requests
{
    public record CommentRequest(int TopicId, string CategoryName, string Content);
}
