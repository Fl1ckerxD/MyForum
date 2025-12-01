using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
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
            var thread = await _uow.Threads.GetThreadWithPostsById(boardShortName, id, cancellationToken);
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

            return thread.Id;
        }
    }
}