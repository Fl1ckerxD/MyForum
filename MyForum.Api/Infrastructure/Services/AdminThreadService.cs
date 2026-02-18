using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Infrastructure.Services
{
    public class AdminThreadService : IAdminThreadService
    {
        private readonly IUnitOfWork _uow;

        public AdminThreadService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>
        /// Полное удаление треда. Удаляет тред и все его посты из базы данных. Использовать с осторожностью, так как восстановление будет невозможно.
        /// </summary>
        public async Task DeleteAsync(int threadId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);

            if (thread == null)
                throw new KeyNotFoundException("Тред не найден");

            await _uow.Threads.DeleteAsync(thread.Id, cancellationToken);
            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Получение списка тредов для админки.
        /// </summary>
        public async Task<IReadOnlyList<AdminThreadDto>> GetThreadsAsync(
            int limit,
            DateTime? cursor,
            string? search = null,
            string? board = null,
            bool? isDeleted = null,
            bool? isLocked = null,
            CancellationToken cancellationToken = default)
        {
            var threads = await _uow.Threads.GetThreadsAsync(
                limit,
                cursor,
                search,
                board,
                isDeleted,
                isLocked,
                cancellationToken);

            return threads.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Блокировка треда. Заблокированный тред нельзя будет комментировать, но он останется видимым для пользователей. Используется для предотвращения спама или неуместного контента без полного удаления треда.
        /// </summary>
        public async Task LockAsync(int threadId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);

            if (thread == null)
                throw new KeyNotFoundException("Тред не найден");

            thread.IsLocked = true;
            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Разблокировка треда. Разблокированный тред можно будет комментировать.
        /// </summary>
        public async Task UnlockAsync(int threadId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdAsync(threadId, cancellationToken);

            if (thread == null)
                throw new KeyNotFoundException("Тред не найден");

            thread.IsLocked = false;
            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Мягкое удаление треда. Помечает тред как удаленный, но не удаляет его из базы данных. Позволяет восстановить тред в случае ошибки.
        /// </summary>
        public async Task SoftDeleteAsync(int threadId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdIncludingDeletedAsync(threadId, cancellationToken);

            if (thread == null)
                throw new KeyNotFoundException("Тред не найден");

            if (thread.IsDeleted)
                return;

            thread.IsDeleted = true;
            thread.DeletedAt = DateTime.UtcNow;

            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Восстановление треда. Восстанавливает ранее удаленный тред.
        /// </summary>
        public async Task RestoreAsync(int threadId, CancellationToken cancellationToken)
        {
            var thread = await _uow.Threads.GetByIdIncludingDeletedAsync(threadId, cancellationToken);

            if (thread == null)
                throw new KeyNotFoundException("Тред не найден");

            if (!thread.IsDeleted)
                throw new InvalidOperationException("Тред не удален");

            thread.IsDeleted = false;
            thread.DeletedAt = null;

            await _uow.SaveAsync(cancellationToken);
        }

        private static AdminThreadDto MapToDto(Thread thread)
        {
            return new AdminThreadDto
            {
                Id = thread.Id,
                BoardShortName = thread.Board.ShortName,
                Title = thread.Subject,
                PostsCount = thread.PostCount,
                IsLocked = thread.IsLocked,
                IsDeleted = thread.IsDeleted,
                CreatedAt = thread.CreatedAt,
                LastBumpAt = thread.LastBumpAt
            };
        }
    }
}