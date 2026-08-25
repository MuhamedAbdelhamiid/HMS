using HMS.Core.Contracts;
using HMS.Core.Entities;
using HMS.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Repository
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
        where TEntity : BaseEntity<TKey>, new()
    {
        private readonly AppDbContext _dbContext;

        public GenericRepository(AppDbContext dbContext)
        => _dbContext = dbContext;
        
        public async Task AddAsync(TEntity entity)
        => await _dbContext.Set<TEntity>().AddAsync(entity);

        public void Delete(TEntity entity)
        => _dbContext.Set<TEntity>().Remove(entity);

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            Expression<Func<TEntity, object>>? orderByDesc = null
            )
        {
            // we hold the table
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if(filter is not null)
                query = query.Where(filter);

            if (orderBy is not null)
                query = query.OrderBy(orderBy);

            if (orderByDesc is not null)
                query = query.OrderByDescending(orderByDesc);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        => await _dbContext.Set<TEntity>().FindAsync(id);

        public void Update(TEntity entity)
        => _dbContext.Set<TEntity>().Update(entity);
    }
}
