namespace WebApplication1.Security;

public class Jwt
{
    public string Token { get; protected set; }
    public long Expiration { get; protected set; }

    public Jwt(string token, long expiration)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Invalid token.");

        if (expiration <= 0)
            throw new ArgumentException("Invalid expiration.");

        Token = token;
        Expiration = expiration;
    }

    public bool IsExpired() => DateTime.UtcNow.Ticks > Expiration;
}