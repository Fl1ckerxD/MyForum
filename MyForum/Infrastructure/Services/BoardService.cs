using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using MyForum.Core.DTOs;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;


namespace MyForum.Infrastructure.Services
{
    public class BoardService : IBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BoardService> _logger;

        public BoardService(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache cache, ILogger<BoardService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }
        public async Task<IReadOnlyCollection<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "AllBoardNames";
            var cachedBoardNames = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedBoardNames))
            {
                _logger.LogDebug("Названия досок получены из кэша");
                return JsonSerializer.Deserialize<IReadOnlyCollection<BoardNamesDto>>(cachedBoardNames)!;
            }

            var boards = await _unitOfWork.Boards.GetAllAsync(cancellationToken);
            var boardNames = _mapper.Map<IReadOnlyCollection<BoardNamesDto>>(boards);
            var serializedBoardNames = JsonSerializer.Serialize(boardNames);
            await _cache.SetStringAsync(cacheKey, serializedBoardNames, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            }, cancellationToken);
            return boardNames;
        }

        public async Task<BoardDto?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default)
        {
            var board = await _unitOfWork.Boards.GetBoardWithThreadsAndPostsAsync(boardShortName, cancellationToken);
            return _mapper.Map<BoardDto?>(board);
        }
    }
}