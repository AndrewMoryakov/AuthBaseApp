using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Data.ViewModel;

namespace WebApplication1.Controllers;
[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class UserController: ControllerBase
{
    private readonly IUserStore _userStore;

    public UserController(IUserStore userStore)
    {
        _userStore = userStore;
    }
    
    
    [HttpGet]
    [Consumes("application/json")]
    public async Task<ActionResult<List<ApplicationUser>>> Get()
    {
        return Ok(_userStore.Get());
    }
    
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<ApplicationUser>> Post([FromBody] UserDto model, CancellationToken ct)
    {
        ApplicationUser user = model.Adapt<ApplicationUser>();

        model = (await _userStore.AddAsync(user, model.Password, ct)).Adapt<UserDto>();;

        return Created($"/api/users/{user.Id}", model);
    }
}