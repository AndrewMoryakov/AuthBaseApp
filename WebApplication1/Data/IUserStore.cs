using Microsoft.AspNetCore.Identity;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data;

public interface IUserStore
{
    public IList<ApplicationUser> Get();
    public Task<ApplicationUser> AddAsync(ApplicationUser user, string password, CancellationToken ct);
}