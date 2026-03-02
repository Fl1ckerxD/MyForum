using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
{
    public class BanRepository : Repository<Ban>, IBanRepository
    {
        public BanRepository(ForumDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Ban>> GetBansAsync(int limit = 50, int? beforeId = null, string? status = null, string? boardShortName = null, CancellationToken cancellationToken = default)
        {
            if (limit < 1)
                limit = 1;

            const int maxLimit = 200;
            if (limit > maxLimit)
                limit = maxLimit;

            var query = _context.Bans
                .AsNoTracking()
                .Include(b => b.Board)
                .AsQueryable();

            if (beforeId.HasValue)
                query = query.Where(b => b.Id < beforeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                var now = DateTime.UtcNow;

                query = normalizedStatus switch
                {
                    "active" => query.Where(b =>
                        b.IsActive &&
                        (b.ExpiresAt == null || b.ExpiresAt > now)
                    ),
                    "expired" => query.Where(b =>
                        b.IsActive &&
                        b.ExpiresAt != null &&
                        b.ExpiresAt <= now
                    ),
                    "revoked" => query.Where(b =>
                        !b.IsActive
                    ),
                    _ => throw new ArgumentException(
                        $"Недопустимое значение статуса: '{status}'. Допустимые значения: 'active', 'expired', 'revoked'",
                        nameof(status)
                    )
                };
            }

            if (!string.IsNullOrEmpty(boardShortName))
                query = query.Where(b => b.Board.ShortName == boardShortName);

            return await query
                .OrderByDescending(b => b.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return await _context.Bans.AnyAsync(b =>
                b.IpAddressHash == ipHash &&
                b.IsActive &&
                (b.ExpiresAt == null || b.ExpiresAt > now) &&
                (b.BoardId == null || b.BoardId == boardId), cancellationToken);
        }
    }
}