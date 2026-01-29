using Mapster;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IStoreOfUsers _storeOfUsers;

    public UserController(IStoreOfUsers storeOfUsers)
    {
        _storeOfUsers = storeOfUsers;
    }
    
    
    [HttpGet]
    [Consumes("application/json")]
    public ActionResult<List<ApplicationUser>> Get()
    {
        return Ok(_storeOfUsers.Get());
    }
    
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<ApplicationUser>> Post([FromBody] UserDto model, CancellationToken ct)
    {
        ApplicationUser user = model.Adapt<ApplicationUser>();

        model = (await _storeOfUsers.AddAsync(user, model.Password, ct)).Adapt<UserDto>();

        return Created($"/api/user/{user.Id}", model);
    }
}
