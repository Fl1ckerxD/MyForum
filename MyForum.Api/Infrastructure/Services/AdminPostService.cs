using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class AdminPostService : IAdminPostService
    {
        private readonly IUnitOfWork _uow;

        public AdminPostService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>
        /// Удаляет пост из базы данных.
        /// </summary>
        public async Task DeleteAsync(int postId, CancellationToken cancellationToken)
        {
            var post = await _uow.Posts.GetByIdIncludingDeletedAsync(postId, cancellationToken);

            if (post == null)
                throw new KeyNotFoundException("Пост не найден");

            await _uow.Posts.DeleteAsync(post.Id, cancellationToken);
            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Получить все посты треда (включая удалённые)
        /// </summary>
        public async Task<IReadOnlyList<AdminPostDto>> GetByThreadAsync(
            int threadId,
            int limit = 50,
            int? afterId = null,
            string? search = null,
            bool? isDeleted = null,
            CancellationToken cancellationToken = default)
        {
            var posts = await _uow.Posts.GetByThreadIncludingDeletedAsync(
                    threadId,
                    limit,
                    afterId,
                    search,
                    isDeleted,
                    cancellationToken);

            return posts.Select(p => new AdminPostDto
            {
                Id = p.Id,
                ThreadId = p.ThreadId,
                BoardId = p.Thread.BoardId,
                IsOriginal = p.IsOriginal,
                Author = p.AuthorName,
                Content = p.Content,
                IsDeleted = p.IsDeleted,
                DeletedAt = p.DeletedAt,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Восстанавливает удалённый пост, устанавливая IsDeleted в false и удаляя DeletedAt.
        /// </summary>
        public async Task RestoreAsync(int postId, CancellationToken cancellationToken)
        {
            var post = await _uow.Posts.GetByIdIncludingDeletedAsync(postId, cancellationToken);

            if (post == null)
                throw new KeyNotFoundException("Пост не найден");

            if (!post.IsDeleted)
                return;

            post.IsDeleted = false;
            post.DeletedAt = null;

            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Мягко удаляет пост, устанавливая IsDeleted в true и заполняя DeletedAt текущей датой и временем.
        /// </summary>
        public async Task SoftDeleteAsync(int postId, CancellationToken cancellationToken)
        {
            var post = await _uow.Posts.GetByIdIncludingDeletedAsync(postId, cancellationToken);

            if (post == null)
                throw new KeyNotFoundException("Пост не найден");

            if (post.IsDeleted)
                return;

            post.IsDeleted = true;
            post.DeletedAt = DateTime.UtcNow;

            await _uow.SaveAsync(cancellationToken);
        }
    }
}