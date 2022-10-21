using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : AggregateRoot<TKey>
{
    IUnitOfWork UnitOfWork { get; }

    IQueryable<TEntity> GetAll();

    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Delete(TEntity entity);
}