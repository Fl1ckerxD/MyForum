using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminBoardService
    {
        Task<IReadOnlyList<BoardDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}