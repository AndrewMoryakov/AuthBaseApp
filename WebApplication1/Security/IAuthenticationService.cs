using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Security;

public interface IAuthenticationService<in TUser> where TUser:IdentityUser
{
    public Jwt CreateAccessTokenAsync(TUser user, string password, CancellationToken cancellationToken);
}