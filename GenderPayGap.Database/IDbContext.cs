using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenderPayGap.Database
{
    public interface IDbContext : IDisposable
    {

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync();
        DatabaseFacade GetDatabase();

    }
}
