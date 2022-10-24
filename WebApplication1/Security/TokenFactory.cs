using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebApplication1.Data.Entities;

namespace WebApplication1.Security;

public class TokenFactory<TUser> : ITokenFactory<TUser> where TUser:IdentityUser
{
    private readonly AuthTokenOptions _tokenTokenOptions;
    private readonly SigningCredentialsKeys _credentialsKeys;

    public TokenFactory(IOptions<AuthTokenOptions> tokenOptionsSnapshot, SigningCredentialsKeys credKeys)
    {
        _tokenTokenOptions = tokenOptionsSnapshot.Value;
        _credentialsKeys = credKeys;
    }
    public Jwt CreateAccessToken(TUser user)
    {
        return BuildAccessToken(user);
    }

    private Jwt BuildAccessToken(TUser user)
    {
        var lifeTimeOfToken = DateTime.UtcNow.AddSeconds(_tokenTokenOptions.Lifetime);

        var securityToken = new JwtSecurityToken
        (
            issuer: _tokenTokenOptions.Issuer,
            audience: _tokenTokenOptions.Audience,
            claims: GetClaims(user),
            expires: lifeTimeOfToken,
            notBefore: DateTime.UtcNow,
            signingCredentials: _credentialsKeys.SigningCredentials
        );

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(securityToken);

        return new Jwt(accessToken, lifeTimeOfToken.Ticks);
    }

    private IEnumerable<Claim> GetClaims(TUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email)
        };
        //ToDo Here can add roles

        return claims;
    }
}