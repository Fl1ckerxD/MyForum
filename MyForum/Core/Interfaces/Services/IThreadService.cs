using MyForum.Core.DTOs;

namespace MyForum.Core.Interfaces.Services
{
    public interface IThreadService
    {
        Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
    }
}