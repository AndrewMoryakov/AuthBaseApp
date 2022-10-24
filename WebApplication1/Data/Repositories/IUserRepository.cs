using WebApplication1.Data.Entities;

namespace WebApplication1.Data.Repositories;

public interface IUserRepository
{
    public IQueryable<ApplicationUser> GetAll();
    public Task AddAsync(ApplicationUser entity, CancellationToken cancellationToken = default);
}