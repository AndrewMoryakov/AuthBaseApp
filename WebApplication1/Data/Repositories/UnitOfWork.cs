using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data.Repositories;

public class UnitOfWork: IUnitOfWork<ApplicationDbContext>
{
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context) => DbContext = context;
    public ApplicationDbContext DbContext { get; set; }
    public async Task SaveChangesAsync(CancellationToken ct) => await DbContext.SaveChangesAsync(ct);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DbContext.Dispose();
            }
        }
        _disposed = true;
    }
}