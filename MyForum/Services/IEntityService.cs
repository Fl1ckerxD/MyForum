﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Services
{
    public interface IEntityService
    {
        Task DeleteEntityAsync<TEntity>(int entityId, Func<ForumContext, DbSet<TEntity>> getDbSet) where TEntity : class;
    }
}
