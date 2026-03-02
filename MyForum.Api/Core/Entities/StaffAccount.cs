using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.Entities
{
    public abstract class StaffAccount
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}