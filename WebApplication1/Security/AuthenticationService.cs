using Microsoft.AspNetCore.Identity;
using WebApplication1.Exceptions;
using WebApplication1.Data.Entities;

namespace WebApplication1.Security;

public class AuthenticationService<TUser> : IAuthenticationService<TUser> where TUser:IdentityUser
{
    private readonly UserManager<TUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(IJwtTokenService jwtTokenService, UserManager<TUser> userManager)
    {
        _jwtTokenService = jwtTokenService;
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

        if (user is not ApplicationUser au)
            throw new InvalidOperationException("Unsupported user type.");

        var issued = _jwtTokenService.CreateUserAccessToken(au);
        return new Jwt(issued.AccessToken, issued.ExpiresAtUtc.Ticks);
    }
}
