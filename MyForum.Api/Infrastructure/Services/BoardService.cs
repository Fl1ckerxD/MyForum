using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;
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

        /// <summary>
        /// Возвращает список названий всех досок
        /// </summary>
        /// <returns>Список названий досок</returns>
        public async Task<IReadOnlyCollection<BoardNamesDto>> GetAllBoardNamesAsync(CancellationToken cancellationToken = default)
        {
            // Попытка получить из кэша
            var cacheKey = "AllBoardNames";

            try
            {
                var cachedBoardNames = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedBoardNames))
                {
                    _logger.LogDebug("Названия досок получены из кэша");
                    return JsonSerializer.Deserialize<IReadOnlyCollection<BoardNamesDto>>(cachedBoardNames)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при получении из кэша, будет использована база данных");
            }

            // Если в кэше нет, получить из базы данных
            var boards = await _unitOfWork.Boards.GetAllAsync(cancellationToken);
            _logger.LogDebug("Названия досок получены из базы данных");

            var boardNames = _mapper.Map<IReadOnlyCollection<BoardNamesDto>>(boards);

            // Сохранить в кэш
            try
            {
                var serializedBoardNames = JsonSerializer.Serialize(boardNames);
                await _cache.SetStringAsync(cacheKey, serializedBoardNames, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                }, cancellationToken);

                _logger.LogDebug("Названия досок сохранены в кэш");

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при сохранении в кэш");
            }

            return boardNames;
        }

        /// <summary>
        /// Возвращает доску с её тредами и постами
        /// </summary>
        /// <param name="boardShortName">Короткое имя доски</param>
        /// <param name="threadLimit">Лимит тредов</param>
        /// <returns>Доска с тредами и постами или null, если доска не найдена</returns>
        public async Task<GetBoardResponse?> GetBoardWithThreadsAndPostsAsync(string boardShortName, int threadLimit = 20, CancellationToken cancellationToken = default)
        {
            try
            {
                var board = await _unitOfWork.Boards.GetBoardWithThreadsAndPostsAsync(boardShortName, threadLimit, cancellationToken);

                if (board is null) return null;

                var boardDto = new BoardDto(
                    Id: board.Id,
                    Name: board.Name,
                    ShortName: board.ShortName,
                    Description: board.Description,
                    CreatedAt: board.CreatedAt,
                    Threads: await Task.WhenAll(board.Threads.Select(async t => await _threadDtoFactory.CreateAsync(t, cancellationToken)))
                );

                DateTime? nextCursor = board.Threads.Count == threadLimit
                    ? board.Threads.Last().LastBumpAt
                    : null;

                return new GetBoardResponse
                {
                    Board = boardDto,
                    NextCursor = nextCursor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доски {ShortName} с лимитом {ThreadLimit}", boardShortName, threadLimit);
                throw;
            }
        }
    }
}