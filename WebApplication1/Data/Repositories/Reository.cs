using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Repositories;

public class Repository<T, TKey> : IRepository<T, TKey>
        where T : AggregateRoot<TKey>
    {
        protected readonly ApplicationDbContext _dbContext;

        protected DbSet<T> DbSet => _dbContext.Set<T>();
        

        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IUnitOfWork UnitOfWork { get; }

        public IQueryable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task AddOrUpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Equals(default(TKey)))//ToDo otptimization
            {
                await AddAsync(entity, cancellationToken);
            }
            else
            {
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedDateTime = DateTimeOffset.Now;
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedDateTime = DateTimeOffset.Now;
            return Task.CompletedTask;
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }
    }