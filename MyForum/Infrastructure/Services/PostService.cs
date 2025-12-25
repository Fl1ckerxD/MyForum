using System.Transactions;
using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.DTOs.Common;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Metrics;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;
        private readonly IIPHasher _ipHasher;
        private readonly IMapper _mapper;
        private readonly IForumMetrics _forumMetrics;

        public PostService(ILogger<PostService> logger, IUnitOfWork uow, IFileService fileService, IIPHasher ipHasher, IMapper mapper, IForumMetrics forumMetrics)
        {
            _logger = logger;
            _uow = uow;
            _fileService = fileService;
            _ipHasher = ipHasher;
            _mapper = mapper;
            _forumMetrics = forumMetrics;
        }

        /// <summary>
        /// Создает пост, привязанный к существующему треду по его ID
        /// </summary>
        public async Task CreateAsync(int threadId, string content, string authorName, string postPassword,
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var post = new Post
            {
                ThreadId = threadId,
                Content = content,
                AuthorName = authorName,
                PostPassword = postPassword,
                UserAgent = userAgent
            };

            await CreateAsync(post, ipAddress, files, cancellationToken);
        }

        /// <summary>
        /// Создает оригинальный пост для нового треда
        /// </summary>
        /// <param name="thread">Объект треда (будет сохранен вместе с постом)</param>
        public async Task CreateAsync(Thread thread, string content, string authorName, string postPassword,
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var post = new Post
            {
                Thread = thread,
                Content = content,
                AuthorName = authorName,
                PostPassword = postPassword,
                UserAgent = userAgent
            };

            thread.OriginalPost = post;

            await CreateAsync(post, ipAddress, files, cancellationToken);
        }

        public async Task<PagedResult<PostDto>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);
            if (thread == null) throw new KeyNotFoundException($"Тред {threadId} не найден");

            var pagedPosts = await _uow.Posts.GetPagedPostsByThreadIdAsync(
                threadId, pageNumber, pageSize, cancellationToken);

            var postDtos = _mapper.Map<List<PostDto>>(pagedPosts.Items);

            return new PagedResult<PostDto>(
                postDtos,
                pagedPosts.TotalCount,
                pagedPosts.PageNumber,
                pagedPosts.PageSize);
        }

        private async Task CreateAsync(Post post, string ipAddress, List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var hashedIp = _ipHasher.HashIP(ipAddress);
            post.IpAddress = hashedIp;

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                if (files != null && files.Any())
                    await ProcessPostFilesAsync(post, files, cancellationToken);

                await _uow.Posts.AddAsync(post, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                transactionScope.Complete();

                _forumMetrics.AddPost();
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