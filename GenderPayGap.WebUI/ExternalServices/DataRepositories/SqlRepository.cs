using System.Linq.Expressions;
using System.Reflection;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

        public IQueryable<T> SqlQueryRaw<T>(string sql)
        {
            return GetDbContext().GetDatabase().SqlQueryRaw<T>(sql);
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

        public ITableDetails<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return new TableDetails<TEntity>(DbContext);
        }

        private class TableDetails<TEntity>(IDbContext dbContext) : ITableDetails<TEntity> where TEntity : class
        {
            public string GetColumnName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                IEntityType entityType = dbContext.Model.FindEntityType(typeof(TEntity));
                if (entityType == null)
                {
                    return null;
                }
            
                MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpression?.Member.Name;

                IEnumerable<IProperty> properties = entityType.GetProperties();
                IProperty property = properties.First(p => p.Name == propertyName);
                return property.GetColumnName();
            }

            public string Name => dbContext.Model.FindEntityType(typeof(TEntity))?.GetSchemaQualifiedTableName();
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
