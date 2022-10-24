using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data.Repositories;

public interface IUnitOfWork<TContext>: IDisposable where TContext : DbContext
{
    TContext DbContext { get; set; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
    //
    // Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, string lockName = null, CancellationToken cancellationToken = default);
    //
    // Task CommitTransactionAsync(CancellationToken cancellationToken = default);--------------------------
}