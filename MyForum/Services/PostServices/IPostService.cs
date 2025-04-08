using Microsoft.AspNetCore.Mvc;
using MyForum.Models;

namespace MyForum.Services.PostServices
{
    public interface IPostService
    {
        Task AddCommentAsync(int topicId, string content, int userId);
        Task ToggleLikeAsync(int postId, int userId);
        Task DeletePostAsync(int postId);
    }
}
