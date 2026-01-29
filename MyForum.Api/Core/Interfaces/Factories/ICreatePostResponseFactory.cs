using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Factories
{
    public interface ICreatePostResponseFactory
    {
        Task<CreatePostResponse> CreateAsync(Post post, CancellationToken cancellationToken = default);
    }
}