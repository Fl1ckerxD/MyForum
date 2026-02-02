namespace MyForum.Api.Core.DTOs.Responses
{
    public class GetBoardResponse
    {
        public BoardDto Board { get; init; } = null!;
        public DateTime? NextCursor { get; init; }
    }
}