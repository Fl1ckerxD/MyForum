namespace MyForum.Api.Core.DTOs
{
    public class BanDto
    {
        public int Id { get; set; }
        public string IpAddressHash { get; set; } = null!;
        public int? BoardId { get; set; }
        public string? BoardShortName { get; set; }
        public string Reason { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public DateTime BannedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}