using System.Diagnostics.Metrics;
using MyForum.Api.Core.Interfaces.Metrics;

namespace MyForum.Api.Core.Metrics
{
    public class ForumMetrics : IForumMetrics
    {
        private readonly Counter<int> _postsCreated;
        private readonly Counter<int> _threadsCreated;
        private readonly UpDownCounter<int> _activeUsers;

        public ForumMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("MyForum.Metrics");

            _postsCreated = meter.CreateCounter<int>(
                name: "forum_posts_created",
                description: "Total number of posts created");

            _threadsCreated = meter.CreateCounter<int>(
                name: "forum_threads_created",
                description: "Total number of threads created");

            _activeUsers = meter.CreateUpDownCounter<int>(
                name: "forum_users_active",
                description: "Current number of active users");
        }

        public void AddPost() => _postsCreated.Add(1);
        public void AddThread() => _threadsCreated.Add(1);
        public void UserLoggedIn() => _activeUsers.Add(1);
        public void UserLoggedOut() => _activeUsers.Add(-1);
    }
}