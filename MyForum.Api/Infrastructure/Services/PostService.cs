using System.Transactions;
using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IIPHasher _ipHasher;
        private readonly IMapper _mapper;
        private readonly IForumMetrics _forumMetrics;
        private readonly ICreatePostResponseFactory _createPostResponseFactory;
        private readonly IPostDtoFactory _postDtoFactory;

        public PostService(ILogger<PostService> logger, IUnitOfWork uow,
            IObjectStorageService objectStorageService, IIPHasher ipHasher,
            IMapper mapper, IForumMetrics forumMetrics, ICreatePostResponseFactory createPostResponseFactory, IPostDtoFactory postDtoFactory)
        {
            _logger = logger;
            _uow = uow;
            _objectStorageService = objectStorageService;
            _ipHasher = ipHasher;
            _mapper = mapper;
            _forumMetrics = forumMetrics;
            _createPostResponseFactory = createPostResponseFactory;
            _postDtoFactory = postDtoFactory;
        }

        /// <summary>
        /// Создает пост, привязанный к существующему треду по его ID
        /// </summary>
        public async Task<CreatePostResponse> CreateAsync(int threadId, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, int? replyToPostId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (replyToPostId.HasValue)
                {
                    var replyToPost = await _uow.Posts.GetByIdAsync(replyToPostId.Value, cancellationToken);
                    if (replyToPost == null || replyToPost.ThreadId != threadId)
                        throw new InvalidOperationException("Пост, на который вы отвечаете, не найден в данном треде.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке поста для ответа. ThreadId: {ThreadId}, ReplyToPostId: {ReplyToPostId}", threadId, replyToPostId);
                throw;
            }

            var post = new Post
            {
                ThreadId = threadId,
                Content = content,
                AuthorName = authorName,
                UserAgent = userAgent,
                ReplyToPostId = replyToPostId
            };

            await CreateAsync(post, ipAddress, files, cancellationToken);

            return await _createPostResponseFactory.CreateAsync(post);
        }

        /// <summary>
        /// Создает оригинальный пост для нового треда
        /// </summary>
        /// <param name="thread">Объект треда (будет сохранен вместе с постом)</param>
        public async Task<int> CreateAsync(Thread thread, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            var post = new Post
            {
                Thread = thread,
                Content = content,
                AuthorName = authorName,
                IsOriginal = true,
                UserAgent = userAgent
            };

            return await CreateAsync(post, ipAddress, files, cancellationToken);
        }

        /// <summary>
        /// Возвращает посты в треде после указанного ID поста с курсорной пагинацией
        /// </summary>
        /// <param name="afterId">ID поста, после которого нужно получить посты</param>
        public async Task<GetPostsResponse> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default)
        {
            try
            {
                var posts = await _uow.Posts.GetPostsAfterIdAsync(threadId, afterId, limit, cancellationToken);

                var postDtos = await Task.WhenAll(posts.Select(p => _postDtoFactory.CreateAsync(p, cancellationToken)));

                int? nextCursor = posts.Count == limit
                    ? posts.Last().Id
                    : null;

                return new GetPostsResponse
                {
                    Posts = postDtos.ToList(),
                    NextCursor = nextCursor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов после Id {AfterId} в треде с Id {ThreadId}", afterId, threadId);
                throw;
            }
        }

        /// <summary>
        /// Сохраняет пост в БД вместе с файлами (если есть) и тредом (если это оригинальный пост)
        /// </summary>
        /// <returns>ID созданного поста</returns>
        private async Task<int> CreateAsync(Post post, string ipAddress, List<IFormFile>? files = null, CancellationToken cancellationToken = default)
        {
            post.IpAddressHash = _ipHasher.HashIP(ipAddress);

            // Используем транзакцию для обеспечения целостности данных
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // Сохраняем файлы в объектном хранилище
                if (files != null && files.Any())
                    await ProcessPostFilesAsync(post, files, cancellationToken);

                await _uow.Posts.AddAsync(post, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                transactionScope.Complete(); // Завершаем транзакцию

                _forumMetrics.AddPost(); // Увеличиваем счетчик постов в метриках
                _logger.LogInformation("Создан новый пост с Id {PostId} в треде с Id {ThreadId}", post.Id, post.ThreadId);

                return post.Id;
            }
            catch (Exception ex)
            {
                await RollbackFileUploadAsync(post); // Откатываем загрузку файлов в случае ошибки
                _logger.LogError(ex, "Ошибка при создании поста. {@Post}", post);
                throw;
            }
        }

        /// <summary>
        /// Сохраняет файлы поста в объектном хранилище
        /// </summary>
        private async Task ProcessPostFilesAsync(Post post, List<IFormFile> files, CancellationToken cancellationToken)
        {
            var postFiles = new List<PostFile>();

            foreach (var file in files)
            {
                try
                {
                    var postFile = await _objectStorageService.SaveFileAsync(file, post, cancellationToken);
                    postFiles.Add(postFile);
                }
                catch (Exception ex)
                {
                    // Откатываем уже сохраненные файлы
                    foreach (var savedFile in postFiles)
                        await _objectStorageService.DeleteFileAsync(savedFile);

                    _logger.LogError(ex, "Ошибка при обработке файла. Файл: {@File}", file.FileName);
                    throw;
                }
            }

            post.Files = postFiles;
        }

        /// <summary>
        /// Удаляем файлы из MinIO если они были созданы
        /// </summary>
        private async Task RollbackFileUploadAsync(Post post)
        {
            try
            {
                if (post.Files?.Any() == true)
                {
                    foreach (var postFile in post.Files)
                        await _objectStorageService.DeleteFileAsync(postFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при откате загрузки файлов");
            }
        }
    }
}