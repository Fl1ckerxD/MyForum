namespace MyForum.Api.Core.DTOs
{
    public class BanDto
    {
        public int Id { get; set; }
        public string IpAddressHash { get; set; } = null!;
        public int? BoardId { get; set; }
        public string Reason { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}