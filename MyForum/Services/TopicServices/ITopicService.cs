using Microsoft.AspNetCore.Mvc;

namespace MyForum.Services.TopicServices
{
    public interface ITopicService
    {
        Task<IActionResult> DeleteTopic(int topicId);
    }
}
