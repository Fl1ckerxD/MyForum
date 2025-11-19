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

        public ThreadService(ILogger<ThreadService> logger, IUnitOfWork uow, IMapper mapper)
        {
            _logger = logger;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            var thread = await _uow.Threads.GetThreadWithPostsById(boardShortName, id, cancellationToken);
            return _mapper.Map<ThreadDto?>(thread);
        }

        public async Task<int> CreateThreadAsync(int boardId, string subject, CancellationToken cancellationToken = default)
        {
            var thread = new Thread
            {
                BoardId = boardId,
                Subject = subject
            };
            try
            {
                await _uow.Threads.AddAsync(thread, cancellationToken);
                await _uow.SaveAsync(cancellationToken);
                return thread.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании треда. {@Thread}", thread);
                throw;
            }
        }
    }
}