namespace MyForum.Infrastructure.Services.PostServices
{
    public interface IPostService
    {
        Task ToggleLikeAsync(int postId, int userId);
    }
}
