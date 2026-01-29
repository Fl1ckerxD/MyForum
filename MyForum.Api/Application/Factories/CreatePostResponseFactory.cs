using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;

namespace MyForum.Api.Application.Factories
{
    public class CreatePostResponseFactory : ICreatePostResponseFactory
    {
        private readonly IFileDtoFactory _fileDtoFactory;

        public CreatePostResponseFactory(IFileDtoFactory fileDtoFactory)
        {
            _fileDtoFactory = fileDtoFactory;
        }

        public async Task<CreatePostResponse> CreateAsync(Post post, CancellationToken cancellationToken = default)
        {
            var fileDtos = post.Files is null
                ? Enumerable.Empty<FileDto>()
                : await Task.WhenAll(
                    post.Files.Select(f => _fileDtoFactory.CreateAsync(f, cancellationToken)));

            return new CreatePostResponse(
                Id: post.Id,
                AuthorName: post.AuthorName,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                Files: fileDtos,
                ReplyToPostId: post.ReplyToPostId
            );
        }
    }
}