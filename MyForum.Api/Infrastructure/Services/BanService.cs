using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Exceptions;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class BanService : IBanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<BanService> _logger;

        public BanService(IUnitOfWork uow, IMapper mapper, ILogger<BanService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Создаёт новый бан для указанного IP-адреса.
        /// </summary>
        /// <param name="ipHash">Хэш IP-адреса, который необходимо забанить.</param>
        /// <param name="boardId">Идентификатор доски. Если <c>null</c>, бан применяется ко всем доскам.</param>
        /// <param name="reason">Причина бана.</param>
        /// <param name="expiresAt">Дата и время окончания бана. Если <c>null</c>, бан бессрочный.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="ipHash"/> или <paramref name="reason"/> равны <c>null</c> или пусты.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="ipHash"/> имеет неверный формат.</exception>
        public async Task BanAsync(string ipHash, int? boardId, string reason, DateTime? expiresAt, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ipHash))
                throw new ArgumentNullException(nameof(ipHash), "Хэш IP-адреса не может быть пустым");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentNullException(nameof(reason), "Причина бана не может быть пустой");
            if (reason.Length < 5)
                throw new ArgumentException("Причина бана должна содержать минимум 5 символов.", nameof(reason));
            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Дата окончания бана должна быть в будущем.", nameof(expiresAt));

            var isAlreadyBanned = await _uow.Bans.IsBannedAsync(ipHash, boardId, cancellationToken);
            if (isAlreadyBanned)
            {
                _logger.LogWarning(
                    "Попытка повторного бана пользователя с IP-хешем {IpHash} для доски {BoardId}",
                    ipHash,
                    boardId);
                throw new UserAlreadyBannedException(
                    $"Пользователь с IP-хешем {ipHash} уже забанен {(boardId.HasValue ? $"на доске {boardId}" : "глобально")}.");
            }

            var ban = new Ban
            {
                IpAddressHash = ipHash,
                BoardId = boardId,
                Reason = reason,
                ExpiresAt = expiresAt
            };

            await _uow.Bans.AddAsync(ban, cancellationToken);
            await _uow.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Создаёт новый бан для автора указанного поста.
        /// </summary>
        /// <param name="postId">Идентификатор поста, автор которого будет забанен.</param>
        /// <param name="boardId">Идентификатор доски. Если <c>null</c>, бан применяется ко всем доскам.</param>
        /// <param name="reason">Причина бана.</param>
        /// <param name="expiresAt">Дата и время окончания бана. Если <c>null</c>, бан бессрочный.</param>
        /// <exception cref="ArgumentException">Выбрасывается, если пост с указанным <paramref name="postId"/> не найден</exception>
        public async Task BanAsync(int postId, int? boardId, string reason, DateTime? expiresAt, CancellationToken cancellationToken)
        {
            if (postId < 0)
                throw new ArgumentException(nameof(postId), "Идентификатор поста должен быть положительным числом.");

            var post = await _uow.Posts.GetByIdIncludingDeletedAsync(postId, cancellationToken);
            if (post == null)
                throw new KeyNotFoundException($"Поста с таким {postId} нет");

            await BanAsync(post.IpAddressHash, boardId, reason, expiresAt, cancellationToken);
        }

        /// <summary>
        /// Получает список банов с поддержкой фильтрации и пагинации.
        /// </summary>
        /// <param name="limit">Максимальное количество возвращаемых банов.</param>
        /// <param name="beforeId">Идентификатор бана для курсорной пагинации.</param>
        /// <param name="isActive">
        /// Фильтр по статусу бана:
        /// <c>true</c> — только активные баны,
        /// <c>false</c> — только неактивные (разбаненные),
        /// <c>null</c> — все баны (по умолчанию).
        /// </param>
        /// <param name="boardId">
        /// Фильтр по доске. 
        /// Если указан, будут возвращены только баны, относящиеся к указанной доске.
        /// Если <c>null</c> — возвращаются как глобальные баны, так и баны конкретных досок.
        /// </param>
        /// <returns>Список банов в виде DTO (<see cref="BanDto"/>), отсортированный по убыванию даты создания.</returns>
        public async Task<IReadOnlyList<BanDto>> GetBansAsync(int limit = 50, int? beforeId = null, string? status = null, string? boardShortName = null, CancellationToken cancellationToken = default)
        {
            var bans = await _uow.Bans.GetBansAsync(limit, beforeId, status, boardShortName, cancellationToken);
            return _mapper.Map<IReadOnlyList<BanDto>>(bans);
        }

        /// <summary>
        /// Проверяет, забанен ли указанный IP-адрес.
        /// </summary>
        /// <param name="ipHash">Хэш IP-адреса для проверки.</param>
        /// <param name="boardId">Идентификатор доски. Если <c>null</c>, проверяется глобальный бан.</param>
        /// <returns>
        /// <c>true</c>, если IP-адрес забанен и бан активен (не истёк и не деактивирован); 
        /// в противном случае <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="ipHash"/> равен <c>null</c> или пуст.</exception>
        public async Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ipHash))
                throw new ArgumentNullException(nameof(ipHash), "Хэш IP-адреса не может быть пустым");

            return await _uow.Bans.IsBannedAsync(ipHash, boardId, cancellationToken);
        }

        /// <summary>
        /// Деактивирует существующий бан (разбанивает).
        /// </summary>
        /// <remarks>
        /// Бан не удаляется из базы данных, а помечается как неактивный (<see cref="Ban.IsActive"/> = <c>false</c>).
        /// Это позволяет сохранить историю банов для аудита.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">Выбрасывается, если бан с указанным <paramref name="banId"/> не найден.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="banId"/> меньше или равен нулю.</exception>
        public async Task UnbanAsync(int banId, CancellationToken cancellationToken)
        {
            if (banId <= 0)
                throw new ArgumentException("Идентификатор бана должен быть больше нуля", nameof(banId));

            var ban = await _uow.Bans.GetByIdAsync(banId, cancellationToken)
                ?? throw new KeyNotFoundException($"Бан с идентификатором {banId} не найден.");

            ban.IsActive = false;
            await _uow.SaveAsync(cancellationToken);
        }
    }
}