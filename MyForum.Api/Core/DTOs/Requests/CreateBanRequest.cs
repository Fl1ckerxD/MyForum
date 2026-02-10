namespace MyForum.Api.Core.DTOs.Requests
{
    public class CreateBanRequest
    {
        public string IpHash { get; set; } = null!;
        public int? BoardId { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
    }
}