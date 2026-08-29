using HMS.Core.Entities;
using System.Linq.Expressions;

namespace HMS.Core.Contracts
{
    public interface IGenericRepository<TEntity, TKey>
        where TEntity : BaseEntity<TKey>, new()
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<TEntity?> GetByIdAsync(TKey id, Expression<Func<TEntity, object>>? includes);
        // will return all without filter or includes
        Task<IEnumerable<TEntity>> GetAllAsync();
        // will return all with filters and ordering
        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            Expression<Func<TEntity, object>>? orderByDesc = null
            );
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
