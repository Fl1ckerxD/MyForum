using MyForum.Core.DTOs;

namespace MyForum.Core.Interfaces.Services
{
    public interface IBoardService
    {
        Task<IReadOnlyCollection<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default);
        Task<BoardDto?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default);
    }
}