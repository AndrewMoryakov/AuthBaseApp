using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Repositories;

namespace WebApplication1.Controllers;

[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CountryController: ControllerBase
{
    private IRepository<Country, Guid> _countryRep;
    private IUnitOfWork<ApplicationDbContext> _uow;
    public CountryController(IRepository<Country, Guid> rep, IUnitOfWork<ApplicationDbContext> uow)
    {
        _countryRep = rep;
        _uow = uow;
    }
    
    [HttpGet()]
    [Consumes("application/json")]
    public async Task<ActionResult<List<Country>>> GetAll()
    {
        var country = await _countryRep.GetAll().ToListAsync();
        return Ok(country);
    }
    
    [HttpGet("{countryId:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<Country>> Get(Guid countryId,  CancellationToken ct = default)
    {
        var country = _countryRep.GetByIdAsync(countryId, ct);
        return Ok(country);
    }
    
    [HttpPost("{name}")]
    [Consumes("application/json")]
    public async Task<ActionResult<Country>> Post(string name, CancellationToken ct = default)
    {
        using (_uow)
        {
            var country = new Country {Title = name};
            await _countryRep.AddAsync(country, ct);
            await _uow.SaveChangesAsync(ct);
            return Ok(country);
        }
    }
}