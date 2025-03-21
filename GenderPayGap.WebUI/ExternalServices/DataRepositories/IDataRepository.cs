using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.Core.Interfaces
{

    public interface IDataRepository : IDisposable
    {

        void Delete<TEntity>(TEntity entity) where TEntity : class;

        TEntity Get<TEntity>(params object[] keyValues) where TEntity : class;

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        void Insert<TEntity>(TEntity entity) where TEntity : class;
        void Insert<TEntity>(List<TEntity> entities) where TEntity : class;
        void ExecuteRawSql(string sql);
        IQueryable<T> SqlQueryRaw<T>(string sql);

        DbSet<TEntity> GetEntities<TEntity>() where TEntity : class;

        void SaveChanges();
        
        ITableDetails<TEntity> GetTable<TEntity>() where TEntity : class;

    }

    public interface ITableDetails<TEntity> where TEntity : class
    {
        string GetColumnName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);
        string Name { get; }
    }

}
