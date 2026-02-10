using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IBanRepository : IRepository<Ban>
    {
        Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken);
    }
}