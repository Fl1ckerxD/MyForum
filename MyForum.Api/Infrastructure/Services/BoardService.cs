using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;


namespace MyForum.Api.Infrastructure.Services
{
    public class BoardService : IBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BoardService> _logger;
        private readonly IThreadDtoFactory _threadDtoFactory;

        public BoardService(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache cache,
            ILogger<BoardService> logger, IThreadDtoFactory threadDtoFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _threadDtoFactory = threadDtoFactory;
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

            if (board is null) return null;

            return new BoardDto(
                Id: board.Id,
                Name: board.Name,
                ShortName: board.ShortName,
                Description: board.Description,
                Threads: await Task.WhenAll(board.Threads.Select(async t => await _threadDtoFactory.CreateAsync(t, cancellationToken)))
            );
        }
    }
}