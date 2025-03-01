using System;
using System.Collections.Generic;

namespace MyForum.Models;

public partial class Post
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public int TopicId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UdatedAt { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual Topic Topic { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
