using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Security;

public interface ITokenFactory<in TUser> where TUser:IdentityUser
{
    Jwt CreateAccessToken(TUser user);
}