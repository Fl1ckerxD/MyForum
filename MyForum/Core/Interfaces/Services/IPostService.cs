using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task CreateAsync(int threadId, string content, string authorName, string postPassword, 
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);

        Task CreateAsync(Thread thread, string content, string authorName, string postPassword, 
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);
    }
}
