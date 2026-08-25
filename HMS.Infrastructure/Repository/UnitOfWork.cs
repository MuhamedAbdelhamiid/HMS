using HMS.Core.Contracts;
using HMS.Core.Entities;
using HMS.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repos;
        private readonly AppDbContext _dbContext;

        public UnitOfWork(AppDbContext dbContext)
        {
            _repos = new Dictionary<Type, object>();
            _dbContext = dbContext;
        }

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>, new()
        {
            var entityType = typeof(TEntity);

            if (_repos.ContainsKey(entityType))
                return (IGenericRepository<TEntity, TKey>) _repos[entityType];
            
            var newRepo = new GenericRepository<TEntity, TKey>(_dbContext);
            _repos[entityType] = newRepo;

            return newRepo;
        }

        public Task<int> SaveChangesAsync()
        => _dbContext.SaveChangesAsync();

    }
}
