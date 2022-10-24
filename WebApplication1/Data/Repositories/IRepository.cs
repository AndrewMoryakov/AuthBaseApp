using WebApplication1.Data.Entities;
using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    IUnitOfWork<ApplicationDbContext> UnitOfWork { get; }
    Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    IQueryable<TEntity?> GetAll();

    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Delete(TEntity entity);
}