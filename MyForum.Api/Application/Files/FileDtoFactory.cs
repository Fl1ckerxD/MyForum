using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Application.Files
{
    public class FileDtoFactory : IFileDtoFactory
    {
        private readonly IObjectStorageService _storage;

        public FileDtoFactory(IObjectStorageService storage)
        {
            _storage = storage;
        }

        public async Task<FileDto> CreateAsync(PostFile file, CancellationToken cancellationToken = default)
        {
            var fileUrlTask = _storage.GetFileUrlAsync(
            file.BucketName,
            file.StoredFileName);

            Task<string?> thumbnailUrlTask = file.ThumbnailKey is null
                ? Task.FromResult<string?>(null)
                : _storage.GetFileUrlAsync(
                    file.BucketName,
                    file.ThumbnailKey);

            await Task.WhenAll(fileUrlTask, thumbnailUrlTask);

            return new FileDto(
                Id: file.Id,
                FileName: file.FileName,
                FileUrl: fileUrlTask.Result,
                ThumbnailUrl: thumbnailUrlTask.Result,
                FileSize: file.FileSize,
                Width: file.Width,
                Height: file.Height
            );
        }
    }
}