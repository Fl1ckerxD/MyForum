namespace MyForum.Api.Core.DTOs.Responses
{
    public class CreatePostResponse
    {
        public long PostId { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}