namespace MyForum.Api.Core.DTOs.Responses
{
    public class GetThreadResponse
    {
        public ThreadDto Thread { get; init; } = null!;
        public int? NextCursor { get; init; }
    }
}