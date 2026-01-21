using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;

namespace MyForum.Api.Application.Factories
{
    public class PostDtoFactory : IPostDtoFactory
    {
        private readonly IFileDtoFactory _fileDtoFactory;

        public PostDtoFactory(IFileDtoFactory fileDtoFactory)
        {
            _fileDtoFactory = fileDtoFactory;
        }

        public async Task<PostDto> CreateAsync(Post post, CancellationToken cancellationToken = default)
        {
            var fileDtos = post.Files is null
                ? Enumerable.Empty<FileDto>()
                : await Task.WhenAll(
                    post.Files.Select(f => _fileDtoFactory.CreateAsync(f, cancellationToken)));

            return new PostDto(
                Id: post.Id,
                AuthorName: post.AuthorName,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                Files: fileDtos
            );
        }
    }
}