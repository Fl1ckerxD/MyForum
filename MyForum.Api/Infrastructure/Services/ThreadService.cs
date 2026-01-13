using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using Serilog;
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

        public ThreadService(ILogger<ThreadService> logger, IUnitOfWork uow, IMapper mapper, IPostService postService, IForumMetrics forumMetrics)
        {
            _logger = logger;
            _uow = uow;
            _forumMetrics = forumMetrics;
            _mapper = mapper;
            _postService = postService;
        }

        public async Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            var thread = await _uow.Threads.GetThreadWithPostsByIdAsync(boardShortName, id, cancellationToken);
            return _mapper.Map<ThreadDto?>(thread);
        }

        public async Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var thread = new Thread
            {
                BoardId = boardId,
                Subject = subject
            };

            await _postService.CreateAsync(thread, postContent, authorName, ipAddress, userAgent, files, cancellationToken);

            _forumMetrics.AddThread();
            _logger.LogInformation("Создан новый тред с Id {ThreadId} на доске с Id {BoardId}", thread.Id, boardId);
            return thread.Id;
        }

        public async Task<PagedResult<ThreadDto>> GetThreadsPagedAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var board = await _uow.Boards.GetByShortNameAsync(boardShortName, cancellationToken);
            if (board == null) throw new KeyNotFoundException($"Доска '{boardShortName}' не найдена");

            var pagedThreads = await _uow.Threads.GetPagedThreadsByBoardWithPostsAsync(
            board.Id, pageNumber, pageSize, cancellationToken);

            var threadDtos = _mapper.Map<List<ThreadDto>>(pagedThreads.Items);
            return new PagedResult<ThreadDto>(threadDtos, pagedThreads.TotalCount, pageNumber, pageSize);
        }
    }
}