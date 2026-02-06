using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Security;

public class AuthTokenOptions
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string Secret { get; set; }

    /// <summary>Access token lifetime in seconds.</summary>
    public int Lifetime { get; set; } = 900;

    /// <summary>Refresh token lifetime in days.</summary>
    public int RefreshLifetimeDays { get; set; } = 30;

    /// <summary>Service (client_credentials) access token lifetime in seconds.</summary>
    public int ServiceLifetime { get; set; } = 900;
}
