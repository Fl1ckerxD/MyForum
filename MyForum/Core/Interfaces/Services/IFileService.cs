using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Services
{
    public interface IFileService
    {
        Task<PostFile> SaveFileAsync(IFormFile file, int postId, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(PostFile postFile, CancellationToken cancellationToken = default);
        string GetFileUrl(string bucketName, string objectKey);
        Task<Stream> GetFileStreamAsync(string bucketName, string objectKey);
    }
}