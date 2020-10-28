using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace GenderPayGap.Core.Classes
{

    public class SqlRepository : IDataRepository
    {

        private IDbContextTransaction Transaction;

        private bool TransactionStarted;

        public SqlRepository(IDbContext context)
        {
            DbContext = context;
        }

        public IDbContext DbContext { get; }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return GetEntities<TEntity>();
        }

        public TEntity Get<TEntity>(params object[] keyValues) where TEntity : class
        {
            return GetEntities<TEntity>().Find(keyValues);
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Add(entity);
        }

        public void Insert<TEntity>(List<TEntity> entities) where TEntity : class
        {
            GetEntities<TEntity>().AddRange(entities);
        }

        public void ExecuteRawSql(string sql)
        {
            GetDbContext().GetDatabase().ExecuteSqlRaw(sql);
        }

        public IDbContext GetDbContext()
        {
            return DbContext;
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Remove(entity);
        }

        public void SaveChanges()
        {
            if (TransactionStarted && Transaction == null)
            {
                Transaction = DbContext.GetDatabase().BeginTransaction();
            }

            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }

        public DbSet<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            return DbContext.Set<TEntity>();
        }

    }

}
