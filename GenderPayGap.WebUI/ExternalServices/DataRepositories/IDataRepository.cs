using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.Core.Interfaces
{

    public interface IDataRepository : IDisposable, IDataTransaction
    {

        void Delete<TEntity>(TEntity entity) where TEntity : class;

        TEntity Get<TEntity>(params object[] keyValues) where TEntity : class;

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        void Insert<TEntity>(TEntity entity) where TEntity : class;
        void Insert<TEntity>(List<TEntity> entities) where TEntity : class;
        void ExecuteRawSql(string sql);

        DbSet<TEntity> GetEntities<TEntity>() where TEntity : class;

        Task SaveChangesAsync();

    }

}
