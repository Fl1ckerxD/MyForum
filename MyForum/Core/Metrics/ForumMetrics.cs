using System.Diagnostics.Metrics;

namespace MyForum.Core.Metrics
{
    public class ForumMetrics
    {
        public static readonly Meter Meter = new("MyForum.Metrics");

        public static readonly Counter<int> PostsCreated = Meter.CreateCounter<int>(
            name: "forum.posts.created",
            unit: "posts",
            description: "Total number of posts created");

        public static readonly Counter<int> ThreadsCreated = Meter.CreateCounter<int>(
            name: "forum.threads.created",
            unit: "threads",
            description: "Total number of threads created");

        public static readonly UpDownCounter<int> ActiveUsers = Meter.CreateUpDownCounter<int>(
            name: "forum.users.active",
            unit: "users",
            description: "Current number of active users");
    }
}