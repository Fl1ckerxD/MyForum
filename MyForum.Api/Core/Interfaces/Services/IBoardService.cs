using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IBoardService
    {
        Task<IReadOnlyCollection<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default);
        Task<BoardDto?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default);
        Task<BoardThreadsResponse> GetThreadsAsync(string boardShortName, DateTime? cursor, int limit = 20, CancellationToken cancellationToken = default);
    }
}