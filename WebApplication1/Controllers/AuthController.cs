using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Repositories;
using WebApplication1.Data.ViewModel;
using WebApplication1.Security;

namespace WebApplication1.Controllers;

[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService<ApplicationUser> _authenticationService;
    readonly UserManager<ApplicationUser> _userManager;
    
    public AuthController(IAuthenticationService<ApplicationUser> authService, UserManager<ApplicationUser> usrManager)
    {
        _authenticationService = authService;
        _userManager = usrManager;
    }

    [HttpPost()]
    public async Task<ActionResult<Jwt>> GetAuth(UserDto user, CancellationToken cancellationToken = default)
    {
        var dbUser = await _userManager.FindByEmailAsync(user.Email);
        var tokenResult = await _authenticationService.CreateAccessTokenAsync(dbUser, user.Password, cancellationToken);
        return Ok(tokenResult);

    }
}
