using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Factories;

namespace MyForum.Api.Application.Factories
{
    public class ThreadDtoFactory : IThreadDtoFactory
    {
        private readonly IPostDtoFactory _postDtoFactory;
        private readonly IMapper _mapper;

        public ThreadDtoFactory(IPostDtoFactory postDtoFactory, IMapper mapper)
        {
            _postDtoFactory = postDtoFactory;
            _mapper = mapper;
        }

        public async Task<ThreadDto> CreateAsync(Core.Entities.Thread thread, CancellationToken cancellationToken = default)
        {
            var originalPost = thread.Posts.FirstOrDefault(p => p.IsOriginal);

            if (originalPost == null)
            {
                throw new InvalidOperationException(
                    $"В треде {thread.Id} нет оригинального поста.");
            }

            return new ThreadDto
            (
                Id: thread.Id,
                Subject: thread.Subject,
                CreatedAt: thread.CreatedAt,
                IsPinned: thread.IsPinned,
                IsLocked: thread.IsLocked,
                LastBumpAt: thread.LastBumpAt,
                OriginalPost: await _postDtoFactory.CreateAsync(thread.Posts.First(p => p.IsOriginal), cancellationToken),
                PostCount: thread.PostCount,
                FileCount: thread.FileCount,
                Board: thread.Board is null ? null : _mapper.Map<BoardSummary>(thread.Board),
                Posts: thread.Posts == null
                    ? Array.Empty<PostDto>()
                    : await Task.WhenAll(
                        thread.Posts
                            .Where(p => !p.IsOriginal)
                            .Select(p => _postDtoFactory.CreateAsync(p, cancellationToken)))
            );
        }
    }
}