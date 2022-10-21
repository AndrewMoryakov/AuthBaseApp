using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Security;

public class AuthOptions
{
    public const string Issuer = "DreamPlace";
    public const string Audience = "http://localhost:58101/";
    const string Key = "mysupersecret_Secretkey!123";
    public const int Lifetime = 1000000;
    
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}