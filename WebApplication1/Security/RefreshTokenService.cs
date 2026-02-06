using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Security;

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(string userId, CancellationToken ct);
    Task<(bool ok, string? userId, string? newRefreshToken)> RotateRefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task<bool> RevokeAsync(string refreshToken, CancellationToken ct);
    string Hash(string token);
}

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _db;
    private readonly AuthTokenOptions _options;

    public RefreshTokenService(ApplicationDbContext db, IOptions<AuthTokenOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<string> CreateRefreshTokenAsync(string userId, CancellationToken ct)
    {
        var token = GenerateToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(token),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshLifetimeDays),
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<(bool ok, string? userId, string? newRefreshToken)> RotateRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var entity = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (entity == null) return (false, null, null);
        if (!entity.IsActive) return (false, null, null);

        var newToken = GenerateToken();
        var newHash = Hash(newToken);

        entity.RevokedAtUtc = DateTime.UtcNow;
        entity.ReplacedByTokenHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = entity.UserId,
            TokenHash = newHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshLifetimeDays),
        });

        await _db.SaveChangesAsync(ct);
        return (true, entity.UserId, newToken);
    }

    public async Task<bool> RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var entity = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (entity == null) return false;
        if (entity.RevokedAtUtc != null) return true;
        entity.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateToken()
    {
        // 32 bytes -> 43 chars base64url (no padding)
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

