using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Factories
{
    public interface IPostDtoFactory
    {
        Task<PostDto> CreateAsync(Post post, CancellationToken cancellationToken = default);
    }
}