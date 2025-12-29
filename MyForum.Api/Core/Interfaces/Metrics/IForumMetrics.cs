namespace MyForum.Api.Core.Interfaces.Metrics
{
    public interface IForumMetrics
    {
        void AddPost();
        void AddThread();
        void UserLoggedIn();
        void UserLoggedOut();
    }
}