using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Data.Entities;
using WebApplication1.Data.ViewModel;
using WebApplication1.Exceptions;
using WebApplication1.Security;

namespace WebApplication1.Controllers;

[Produces("application/json")]
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService<ApplicationUser> _authenticationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IJwtTokenService _jwtTokens;
    private readonly ServiceClientOptions _serviceClients;
    private readonly AuthTokenOptions _tokenOptions;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    
    public AuthController(
        IAuthenticationService<ApplicationUser> authService,
        UserManager<ApplicationUser> usrManager,
        IRefreshTokenService refreshTokens,
        IJwtTokenService jwtTokens,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        Microsoft.Extensions.Options.IOptions<ServiceClientOptions> serviceClients,
        Microsoft.Extensions.Options.IOptions<AuthTokenOptions> tokenOptions)
    {
        _authenticationService = authService;
        _userManager = usrManager;
        _refreshTokens = refreshTokens;
        _jwtTokens = jwtTokens;
        _signInManager = signInManager;
        _configuration = configuration;
        _serviceClients = serviceClients.Value;
        _tokenOptions = tokenOptions.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserDto user, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(user.Email);
        if (existing != null)
            return Conflict(new { message = "User already exists." });

        var newUser = new ApplicationUser
        {
            UserName = user.Email,
            Email = user.Email
        };

        var res = await _userManager.CreateAsync(newUser, user.Password);
        if (!res.Succeeded)
            return BadRequest(new { message = "Registration failed.", errors = res.Errors.Select(e => e.Description).ToList() });

        return Ok(new { userId = newUser.Id, email = newUser.Email });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserDto user, CancellationToken ct)
    {
        var dbUser = await _userManager.FindByEmailAsync(user.Email);
        if (dbUser == null)
            return Unauthorized(new { message = "Invalid credentials." });

        Jwt jwt;
        try
        {
            jwt = await _authenticationService.CreateAccessTokenAsync(dbUser, user.Password, ct);
        }
        catch (NotFoundEntityException)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var refresh = await _refreshTokens.CreateRefreshTokenAsync(dbUser!.Id, ct);
        SetRefreshCookie(refresh);

        return Ok(new { accessToken = jwt.Token, expiresAtUtcTicks = jwt.Expiration });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("tca.refresh", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Missing refresh token." });

        var (ok, userId, newRefresh) = await _refreshTokens.RotateRefreshTokenAsync(refreshToken, ct);
        if (!ok || userId == null || newRefresh == null)
            return Unauthorized(new { message = "Invalid refresh token." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized(new { message = "User not found." });

        var issued = _jwtTokens.CreateUserAccessToken(user);
        SetRefreshCookie(newRefresh);
        return Ok(new { accessToken = issued.AccessToken, expiresAtUtcTicks = issued.ExpiresAtUtc.Ticks });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("tca.refresh", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _refreshTokens.RevokeAsync(refreshToken, ct);
        }

        Response.Cookies.Delete("tca.refresh", new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });

        return Ok(new { ok = true });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        return Ok(new { userId, email });
    }

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult Google([FromQuery] string? returnUrl = null)
    {
        // Fail fast if Google isn't configured.
        if (string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]) ||
            string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]))
        {
            return StatusCode(501, new { message = "Google authentication is not configured." });
        }

        var safeReturnUrl = SanitizeReturnUrl(returnUrl);
        var redirectUrl = Url.Action(nameof(GoogleCallback), new { returnUrl = safeReturnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl!);
        return Challenge(props, "Google");
    }

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null, CancellationToken ct = default)
    {
        var safeReturnUrl = SanitizeReturnUrl(returnUrl);

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return Redirect(safeReturnUrl);
        }

        var email =
            info.Principal.FindFirstValue(ClaimTypes.Email) ??
            info.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(safeReturnUrl);
        }

        // 1) Existing user by external login?
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        // 2) Existing user by email?
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);
        }

        // 3) Create user if needed
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return Redirect(safeReturnUrl);
            }
        }

        // Attach external login if not already attached.
        var existingLogin = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (existingLogin == null)
        {
            var addLogin = await _userManager.AddLoginAsync(user, info);
            // If it's already attached (race), ignore.
            if (!addLogin.Succeeded && addLogin.Errors.All(e => e.Code != "LoginAlreadyAssociated"))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return Redirect(safeReturnUrl);
            }
        }

        // Clear external cookie and issue refresh token for SPA.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        var refresh = await _refreshTokens.CreateRefreshTokenAsync(user.Id, ct);
        SetRefreshCookie(refresh);

        return Redirect(safeReturnUrl);
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult Token([FromBody] ClientCredentialsRequest request)
    {
        var client = _serviceClients.Clients.Values.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null || client.ClientSecret != request.ClientSecret)
            return Unauthorized(new { message = "Invalid client credentials." });

        var issued = _jwtTokens.CreateServiceAccessToken(client.ClientId, client.Scopes);
        return Ok(new { accessToken = issued.AccessToken, expiresAtUtcTicks = issued.ExpiresAtUtc.Ticks });
    }

    private void SetRefreshCookie(string token)
    {
        var opts = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(_tokenOptions.RefreshLifetimeDays)
        };
        Response.Cookies.Append("tca.refresh", token, opts);
    }

    private static string SanitizeReturnUrl(string? returnUrl)
    {
        // Prevent open redirects. For now we allow only localhost/127.0.0.1 (dev).
        const string fallback = "http://127.0.0.1:5173/login";
        if (string.IsNullOrWhiteSpace(returnUrl))
            return fallback;

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            return fallback;

        if (uri.Scheme is not ("http" or "https"))
            return fallback;

        if (uri.Host is not ("127.0.0.1" or "localhost"))
            return fallback;

        return uri.ToString();
    }

    public sealed record ClientCredentialsRequest(string ClientId, string ClientSecret);
}
