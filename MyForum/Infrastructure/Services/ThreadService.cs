using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;

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
    }
}