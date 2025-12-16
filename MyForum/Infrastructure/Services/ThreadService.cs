using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.DTOs.Common;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
using MyForum.Core.Metrics;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Infrastructure.Services
{
    public class ThreadService : IThreadService
    {
        private readonly ILogger<ThreadService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPostService _postService;

        public ThreadService(ILogger<ThreadService> logger, IUnitOfWork uow, IMapper mapper, IPostService postService)
        {
            _logger = logger;
            _uow = uow;
            _mapper = mapper;
            _postService = postService;
        }

        public async Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            var thread = await _uow.Threads.GetThreadWithPostsByIdAsync(boardShortName, id, cancellationToken);
            return _mapper.Map<ThreadDto?>(thread);
        }

        public async Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string postPassword, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var thread = new Thread
            {
                BoardId = boardId,
                Subject = subject
            };

            await _postService.CreateAsync(thread, postContent, authorName, postPassword, ipAddress, userAgent, files, cancellationToken);

            ForumMetrics.ThreadsCreated.Add(1);
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