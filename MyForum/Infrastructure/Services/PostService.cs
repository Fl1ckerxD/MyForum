using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;
        private readonly IIPHasher _ipHasher;

        public PostService(ILogger<PostService> logger, IUnitOfWork uow, IFileService fileService, IIPHasher ipHasher)
        {
            _logger = logger;
            _uow = uow;
            _fileService = fileService;
            _ipHasher = ipHasher;
        }

        public async Task CreateAsync(int threadId, string content, string authorName, string postPassword,
            bool isOriginalPost, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default)
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

                if (files != null && files.Any())
                    await ProcessPostFilesAsync(post.Id, files, cancellationToken);

                if (isOriginalPost)
                    await UpdateThreadOriginalPostAsync(threadId, post.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await RollbackFileUploadAsync(post.Id);
                _logger.LogError(ex, "Ошибка при создании поста. {@Post}", post);
                throw;
            }
        }

        private async Task ProcessPostFilesAsync(int postId, List<IFormFile> files, CancellationToken cancellationToken)
        {
            foreach (var file in files)
            {
                try
                {
                    var postFile = await _fileService.SaveFileAsync(file, postId, cancellationToken);
                    await _uow.PostFiles.AddAsync(postFile, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке файла для поста с ID '{PostId}'. Файл: {@File}", postId, file.FileName);
                    throw;
                }
            }

            await _uow.SaveAsync(cancellationToken);
        }

        private async Task UpdateThreadOriginalPostAsync(int threadId, int postId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);
            if (thread != null)
            {
                thread.OriginalPostId = postId;
                _uow.Threads.Update(thread);
                await _uow.SaveAsync(cancellationToken);
            }
        }

        private async Task RollbackFileUploadAsync(int postId)
        {
            try
            {
                var postFiles = await _uow.PostFiles.GetByPostIdAsync(postId);
                
                foreach (var postFile in postFiles)
                    await _fileService.DeleteFileAsync(postFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при откате загрузки файлов для поста {PostId}", postId);
            }
        }
    }
}