using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IBoardService
    {
        Task<IReadOnlyCollection<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default);
        Task<GetBoardResponse?> GetBoardWithThreadsAndPostsAsync(string boardShortName, int threadLimit = 20, CancellationToken cancellationToken = default);
    }
}