using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Factories
{
    public interface IFileDtoFactory
    {
        Task<FileDto> CreateAsync(PostFile file, CancellationToken cancellationToken = default);
    }
}