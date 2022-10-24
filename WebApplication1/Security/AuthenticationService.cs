using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using WebApplication1.Exceptions;

namespace WebApplication1.Security;

public class AuthenticationService<TUser> : IAuthenticationService<TUser> where TUser:IdentityUser
{
    private readonly IPasswordHasher<TUser> _passwordHasher;
    private readonly ITokenFactory<TUser> _tokenFactory;

    public AuthenticationService(IPasswordHasher<TUser> passwordHasher, ITokenFactory<TUser> tokenFactory)
    {
        _passwordHasher = passwordHasher;
        _tokenFactory = tokenFactory;
    }

    public Jwt CreateAccessTokenAsync(TUser user, string password, 
        CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHasher.HashPassword(user, password);

        if (_passwordHasher.VerifyHashedPassword(user, hashedPassword, password) 
            == PasswordVerificationResult.Failed)
             throw new NotFoundEntityException("User not found.");

        return _tokenFactory.CreateAccessToken(user);
    }
}