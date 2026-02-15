using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
{
    public class StaffAccountRepository : IStaffAccountRepository
    {
        private readonly ForumDbContext _context;

        public StaffAccountRepository(ForumDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StaffAccount account, CancellationToken cancellationToken = default)
        {
            await _context.StaffAccounts.AddAsync(account);
        }

        public async Task<StaffAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.StaffAccounts.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
        }
    }
}