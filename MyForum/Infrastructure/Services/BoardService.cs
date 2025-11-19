using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Infrastructure.Services
{
    public class BoardService : IBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BoardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default)
        {
            var boards = await _unitOfWork.Boards.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<BoardNamesDto>>(boards);
        }

        public async Task<BoardDto?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default)
        {
            var board = await _unitOfWork.Boards.GetBoardWithThreadsAndPostsAsync(boardShortName, cancellationToken);
            return _mapper.Map<BoardDto?>(board);
        }
    }
}