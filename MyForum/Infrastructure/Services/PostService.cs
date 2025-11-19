using AutoMapper;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IIPHasher _ipHasher;

        public PostService(ILogger<PostService> logger, IUnitOfWork uow, IMapper mapper, IIPHasher ipHasher)
        {
            _logger = logger;
            _uow = uow;
            _mapper = mapper;
            _ipHasher = ipHasher;
        }

        public async Task CreateAsync(int threadId, string content,
            string authorName, string postPassword,
            bool isOriginalPost, string ipAddress,
            string userAgent, CancellationToken cancellationToken = default)
        {
            var hashedIp = _ipHasher.HashIP(ipAddress);

            var post = new Post
            {
                ThreadId = threadId,
                Content = content,
                AuthorName = authorName,
                PostPassword = postPassword,
                IpAddress = hashedIp,
                UserAgent = userAgent,
            };
            
            try
            {
                await _uow.Posts.AddAsync(post, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                if (isOriginalPost)
                {
                    var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);
                    if (thread != null)
                    {
                        thread.OriginalPostId = post.Id;
                        _uow.Threads.Update(thread);
                        await _uow.SaveAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании поста. {@Post}", post);
                throw;
            }
        }
    }
}