namespace MyForum.Api.Core.DTOs.Responses
{
    public class GetThreadsResponse
    {
        public List<ThreadDto> Threads { get; init; } = [];
        public DateTime? NextCursor { get; init; }
    }
}