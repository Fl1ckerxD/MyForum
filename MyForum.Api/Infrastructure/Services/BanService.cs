using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class BanService : IBanService
    {
        private readonly IUnitOfWork _uow;

        public BanService(IUnitOfWork uow)
        {
            _uow = uow;
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