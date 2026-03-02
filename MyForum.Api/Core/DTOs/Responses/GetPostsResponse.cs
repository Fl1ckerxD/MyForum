namespace MyForum.Api.Core.DTOs.Responses
{
    public class GetPostsResponse
    {
        public IReadOnlyCollection<PostDto> Posts { get; init; } = [];
        public int? NextCursor { get; init; }
    }
}