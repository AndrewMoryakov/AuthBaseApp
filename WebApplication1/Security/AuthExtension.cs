using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Security;

public static class AuthExtension
{
    public static void ConfigureJwtAuthentication(this IServiceCollection services, AuthTokenOptions tokenTokenOptions, SigningCredentialsKeys credKeys)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.TokenValidationParameters =
                    new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = tokenTokenOptions.Issuer,
                        ValidAudience = tokenTokenOptions.Audience,
                        IssuerSigningKey = credKeys.SecurityKey,
                        ClockSkew = TimeSpan.Zero
                    };
            });
    }
}