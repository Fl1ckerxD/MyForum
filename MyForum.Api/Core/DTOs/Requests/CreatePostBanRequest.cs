namespace MyForum.Api.Core.DTOs.Requests
{
    public class CreatePostBanRequest
    {
        public int? BoardId { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
    }
}