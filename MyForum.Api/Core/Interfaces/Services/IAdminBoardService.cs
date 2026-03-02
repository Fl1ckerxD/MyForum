using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminBoardService
    {
        Task<IReadOnlyList<BoardDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateVisibilityAsync(int id, bool isHidden, CancellationToken cancellationToken = default);
    }
}