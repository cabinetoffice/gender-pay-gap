using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenderPayGap.Database
{
    public interface IDbContext : IDisposable
    {

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        void SaveChanges();
        DatabaseFacade GetDatabase();

    }
}
