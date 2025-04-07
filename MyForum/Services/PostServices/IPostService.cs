using Microsoft.AspNetCore.Mvc;

namespace MyForum.Services.PostServices
{
    public interface IPostService
    {
        Task<IActionResult> DeleteTopic(int topicId);
    }
}
