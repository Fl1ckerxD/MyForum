namespace MyForum.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task CreateAsync(int threadId, string content,
            string authorName, string postPassword,
            bool isOriginalPost, string ipAddress,
            string userAgent, CancellationToken cancellationToken = default);
    }
}
