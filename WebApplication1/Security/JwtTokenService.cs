using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebApplication1.Data.Entities;

namespace WebApplication1.Security;

public interface IJwtTokenService
{
    JwtIssueResult CreateUserAccessToken(ApplicationUser user);
    JwtIssueResult CreateServiceAccessToken(string clientId, IReadOnlyList<string> scopes);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly AuthTokenOptions _options;
    private readonly SigningCredentialsKeys _signing;

    public JwtTokenService(IOptions<AuthTokenOptions> tokenOptionsSnapshot, SigningCredentialsKeys signing)
    {
        _options = tokenOptionsSnapshot.Value;
        _signing = signing;
    }

    public JwtIssueResult CreateUserAccessToken(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var expiresAtUtc = DateTime.UtcNow.AddSeconds(_options.Lifetime);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("typ", "user")
        };

        return Issue(claims, expiresAtUtc);
    }

    public JwtIssueResult CreateServiceAccessToken(string clientId, IReadOnlyList<string> scopes)
    {
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("ClientId is required.", nameof(clientId));

        var expiresAtUtc = DateTime.UtcNow.AddSeconds(_options.ServiceLifetime);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, clientId),
            new(JwtRegisteredClaimNames.Sub, clientId),
            new("typ", "service"),
        };

        foreach (var scope in scopes ?? Array.Empty<string>())
            claims.Add(new Claim("scope", scope));

        return Issue(claims, expiresAtUtc);
    }

    private JwtIssueResult Issue(IEnumerable<Claim> claims, DateTime expiresAtUtc)
    {
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: _signing.SigningCredentials);

        var handler = new JwtSecurityTokenHandler();
        var raw = handler.WriteToken(token);
        return new JwtIssueResult(raw, expiresAtUtc);
    }
}

public sealed record JwtIssueResult(string AccessToken, DateTime ExpiresAtUtc);

