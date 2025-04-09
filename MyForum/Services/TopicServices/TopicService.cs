
using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Services.TopicServices
{
    public class TopicService : ITopicService
    {
        private readonly ForumContext _context;
        private readonly IEntityService _entityService;
        public TopicService(ForumContext context, IEntityService entityService)
        {
            _context = context;  
            _entityService = entityService;
        }

        public async Task CreateTopicAsync(int categoryId, string title, string content, int userId)
        {
            var topic = new Topic
            {
                Title = title,
                Content = content,
                CategoryId = categoryId,
                UserId = userId
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTopic(int topicId)
        {
            await _entityService.DeleteEntityAsync(topicId, context => context.Topics);
        }

        public async Task<Topic> GetTopicByIdAsync(int topicId)
        {
            return await _context.Topics
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Posts).ThenInclude(x => x.User)
                .Include(x => x.Posts).ThenInclude(x => x.Likes)
                .FirstOrDefaultAsync(t => t.Id == topicId);
        }
    }
}
