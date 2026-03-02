using AutoMapper;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Infrastructure.Services
{
    public class ThreadService : IThreadService
    {
        private readonly ILogger<ThreadService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPostService _postService;
        private readonly IForumMetrics _forumMetrics;
        private readonly IThreadDtoFactory _threadDtoFactory;

        public ThreadService(ILogger<ThreadService> logger, IUnitOfWork uow,
            IMapper mapper, IPostService postService,
            IForumMetrics forumMetrics, IThreadDtoFactory threadDtoFactory)
        {
            _logger = logger;
            _uow = uow;
            _forumMetrics = forumMetrics;
            _mapper = mapper;
            _postService = postService;
            _threadDtoFactory = threadDtoFactory;
        }

        /// <summary>
        /// Возвращает тред с постами по короткому имени доски и ID треда
        /// </summary>
        public async Task<GetThreadResponse?> GetThreadWithPostsByIdAsync(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            try
            {
                int postLimit = 20;

                var thread = await _uow.Threads.GetThreadWithPostsByIdAsync(boardShortName, id, postLimit, cancellationToken);

                if (thread is null) return null;

                var threadDto = await _threadDtoFactory.CreateAsync(thread, cancellationToken);

                int? nextCursor = thread.Posts.Count == postLimit
                    ? thread.Posts.Last().Id
                    : null;

                return new GetThreadResponse
                {
                    Thread = threadDto,
                    NextCursor = nextCursor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении треда с Id {ThreadId} на доске {BoardShortName}", id, boardShortName);
                throw;
            }
        }

        /// <summary>
        /// Создает новый тред с постом
        /// </summary>
        /// <returns>ID созданного треда</returns>
        public async Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var thread = new Thread
            {
                BoardId = boardId,
                Subject = subject
            };

            try
            {
                await _postService.CreateAsync(thread, postContent, authorName, ipAddress, userAgent, files, cancellationToken);

                _forumMetrics.AddThread();
                _logger.LogInformation("Создан новый тред с Id {ThreadId} на доске с Id {BoardId}", thread.Id, boardId);

                return thread.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании треда на доске с Id {BoardId}", boardId);
                throw;
            }
        }

        public async Task<GetThreadsResponse> GetThreadsByCursorAsync(string boardShortName, DateTime? cursor, int limit = 20, CancellationToken cancellationToken = default)
        {
            try
            {
                var threads = await _uow.Threads.GetThreadsByCursorWithPostsAsync(boardShortName, cursor, limit, cancellationToken);

                var threadDtos = await Task.WhenAll(threads.Select(t => _threadDtoFactory.CreateAsync(t, cancellationToken)));

                DateTime? nextCursor = threads.Count == limit
                    ? threads.Last().LastBumpAt
                    : null;

                return new GetThreadsResponse
                {
                    Threads = threadDtos.ToList(),
                    NextCursor = nextCursor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тредов на доске {BoardShortName} по курсору", boardShortName);
                throw;
            }
        }
    }
}