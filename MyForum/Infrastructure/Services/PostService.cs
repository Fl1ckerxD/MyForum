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

                if (files != null && files.Any())
                    await ProcessPostFilesAsync(post, files, cancellationToken);

                await _uow.Posts.AddAsync(post, cancellationToken);

                if (isOriginalPost)
                    await UpdateThreadOriginalPostAsync(threadId, post, cancellationToken);

                await _uow.SaveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await RollbackFileUploadAsync(post);
                _logger.LogError(ex, "Ошибка при создании поста. {@Post}", post);
                throw;
            }
        }

        private async Task ProcessPostFilesAsync(Post post, List<IFormFile> files, CancellationToken cancellationToken)
        {
            var postFiles = new List<PostFile>();

            foreach (var file in files)
            {
                try
                {
                    var postFile = await _fileService.SaveFileAsync(file, post, cancellationToken);
                    postFiles.Add(postFile);
                }
                catch (Exception ex)
                {
                    // Откатываем уже сохраненные файлы
                    foreach (var savedFile in postFiles)
                        await _fileService.DeleteFileAsync(savedFile);

                    _logger.LogError(ex, "Ошибка при обработке файла. Файл: {@File}", file.FileName);
                    throw;
                }
            }

            post.Files = postFiles;
        }

        private async Task UpdateThreadOriginalPostAsync(int threadId, Post post, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);
            if (thread != null)
            {
                thread.OriginalPost = post;
                _uow.Threads.Update(thread);
            }
        }

        private async Task RollbackFileUploadAsync(Post post)
        {
            try
            {
                // Удаляем файлы из MinIO если они были созданы
                if (post.Files?.Any() == true)
                {
                    foreach (var postFile in post.Files)
                        await _fileService.DeleteFileAsync(postFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при откате загрузки файлов");
            }
        }
    }
}