using Microsoft.AspNetCore.Identity;
using WebApplication1.Exceptions;

namespace WebApplication1.Security;

public class AuthenticationService<TUser> : IAuthenticationService<TUser> where TUser:IdentityUser
{
    private readonly ITokenFactory<TUser> _tokenFactory;
    private readonly UserManager<TUser> _userManager;

    public AuthenticationService(ITokenFactory<TUser> tokenFactory, UserManager<TUser> userManager)
    {
        _tokenFactory = tokenFactory;
        _userManager = userManager;
    }

    public async Task<Jwt> CreateAccessTokenAsync(TUser user, string password, 
        CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new NotFoundEntityException("User not found.");
        }

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isValid)
        {
            throw new NotFoundEntityException("User not found.");
        }

        return _tokenFactory.CreateAccessToken(user);
    }
}
