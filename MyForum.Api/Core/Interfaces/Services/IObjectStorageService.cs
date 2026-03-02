using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IObjectStorageService
    {
        Task<PostFile> SaveFileAsync(IFormFile file, Post post, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(PostFile postFile, CancellationToken cancellationToken = default);
        Task<string?> GetFileUrlAsync(string bucketName, string objectKey, int expiresSeconds = 3600);
        Task<Stream> GetFileStreamAsync(string bucketName, string objectKey);
    }
}