namespace MyForum.Api.Core.DTOs.Responses
{
    public class CreateThreadResponse
    {
        public long ThreadId { get; init; }
        public string BoardShortName { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}