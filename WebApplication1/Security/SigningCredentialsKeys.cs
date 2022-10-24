using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Security;

public class SigningCredentialsKeys
{
    public SecurityKey SecurityKey { get; }
    public SigningCredentials SigningCredentials { get; }

    public SigningCredentialsKeys(string key)
    {
        var rawKeys = Encoding.ASCII.GetBytes(key);

        SecurityKey = new SymmetricSecurityKey(rawKeys);
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);
    }
}