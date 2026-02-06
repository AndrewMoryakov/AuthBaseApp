using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data.Entities;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the refresh token string (base64url).
    /// We never store the raw refresh token in the database.
    /// </summary>
    [Required]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
}

