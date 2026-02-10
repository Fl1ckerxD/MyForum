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