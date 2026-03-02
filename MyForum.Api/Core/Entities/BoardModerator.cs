namespace MyForum.Api.Core.Entities
{
    public class BoardModerator : StaffAccount
    {
        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public ModeratorPermissions Permissions { get; set; } = new();
    }
}