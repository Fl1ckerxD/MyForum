using System;
using System.Collections.Generic;

namespace MyForum.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
