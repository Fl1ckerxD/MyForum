using System.Diagnostics.Metrics;

namespace MyForum.Core.Metrics
{
    public class ForumMetrics
    {
        private readonly Counter<int> _postsCreated;
        private readonly Counter<int> _threadsCreated;
        private readonly UpDownCounter<int> _activeUsers;

        public ForumMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("MyForum.Metrics");

            _postsCreated = meter.CreateCounter<int>(
                name: "forum.posts.created",
                unit: "posts",
                description: "Total number of posts created");

            _threadsCreated = meter.CreateCounter<int>(
                name: "forum.threads.created",
                unit: "threads",
                description: "Total number of threads created");

            _activeUsers = meter.CreateUpDownCounter<int>(
                name: "forum.users.active",
                unit: "users",
                description: "Current number of active users");
        }

        public void AddPost(string boardName) => _postsCreated.Add(1, new KeyValuePair<string, object?>("board", boardName));
        public void AddThread() => _threadsCreated.Add(1);
        public void UserLoggedIn() => _activeUsers.Add(1);
        public void UserLoggedOut() => _activeUsers.Add(-1);
    }
}