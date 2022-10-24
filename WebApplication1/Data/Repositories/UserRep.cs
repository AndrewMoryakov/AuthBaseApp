using Microsoft.AspNetCore.Identity;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Repositories;

public class UserRep : IUserRepository
{
    private IUserStore<ApplicationUser> _systemUsrStore;
    public IUnitOfWork<ApplicationDbContext> UnitOfWork { get; }

    public UserRep(IUserStore<ApplicationUser> systemUsrStore, IUnitOfWork<ApplicationDbContext> uow)
    {
        _systemUsrStore = systemUsrStore;
        UnitOfWork = uow;
    }
    
    public IQueryable<ApplicationUser> GetAll()
    {
        return UnitOfWork.DbContext.Users;
    }
    
    public async Task AddAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
    {
        _ = await _systemUsrStore.CreateAsync(entity, cancellationToken);   ;
    }
}