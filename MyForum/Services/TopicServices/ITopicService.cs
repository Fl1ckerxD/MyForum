using Microsoft.AspNetCore.Mvc;
using MyForum.Models;

namespace MyForum.Services.TopicServices
{
    public interface ITopicService
    {
        Task<Topic> GetTopicByIdAsync(int topicId);
        Task CreateTopicAsync(int categoryId, string title, string content, int userId);
        Task DeleteTopic(int topicId);
    }
}
