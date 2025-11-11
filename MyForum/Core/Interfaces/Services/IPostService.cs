namespace MyForum.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task ToggleLikeAsync(int postId, int userId);
    }
}
