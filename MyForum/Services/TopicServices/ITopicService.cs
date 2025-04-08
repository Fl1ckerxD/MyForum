using Microsoft.AspNetCore.Mvc;
using MyForum.Models;

namespace MyForum.Services.TopicServices
{
    public interface ITopicService
    {
        Task DeleteTopic(int topicId);
        Task<Topic> GetTopicByIdAsync(int topicId);
    }
}
