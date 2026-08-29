using HMS.Core.Entities;

namespace HMS.Core.Contracts
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>, new();

        Task<int> SaveChangesAsync();
    }
}
