using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Repositories;

namespace WebApplication1.Controllers;

[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IRepository<ApplicationUser, string> _repository;
    
    public AuthController(IRepository<ApplicationUser, string> repos)
    {
        _repository = repos;
    }

    [HttpGet()]
    public async Task<ActionResult<string>> GetAuth()
    {
        return Ok(_repository.GetAll());
    }
}