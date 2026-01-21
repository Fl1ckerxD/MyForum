using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyForum.Api.Core.DTOs;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Factories
{
    public interface IThreadDtoFactory
    {
        Task<ThreadDto> CreateAsync(Thread thread, CancellationToken cancellationToken = default);
    }
}