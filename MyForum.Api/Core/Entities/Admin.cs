namespace MyForum.Api.Core.Entities
{
    public class Admin : StaffAccount
    {
        public bool CanManageBoards { get; set; } = true;
        public bool CanManageModerators { get; set; } = true;
    }
}