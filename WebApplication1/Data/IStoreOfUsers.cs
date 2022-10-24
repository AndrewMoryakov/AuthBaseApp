using Microsoft.AspNetCore.Identity;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data;

public interface IStoreOfUsers
{
    public IList<ApplicationUser> Get();
    public Task<ApplicationUser> AddAsync(ApplicationUser user, string password, CancellationToken ct);
}