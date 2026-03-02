using Microsoft.EntityFrameworkCore;

namespace MyForum.Api.Core.Entities
{
    [Owned]
    public class ModeratorPermissions
    {
        public bool DeletePosts { get; set; } = true;
        public bool BanUsers { get; set; } = true;
        public bool ManageThreads { get; set; } = true;
    }
}