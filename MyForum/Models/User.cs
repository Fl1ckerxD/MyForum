using System.ComponentModel.DataAnnotations;

namespace MyForum.Models;

public partial class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
