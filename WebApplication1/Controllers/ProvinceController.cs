using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Repositories;
using WebApplication1.Data.ViewModel;

namespace WebApplication1.Controllers;

[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProvinceController: ControllerBase
{
    private IRepository<Province, Guid> _countryRep;
    private IUnitOfWork<ApplicationDbContext> _uow;
    public ProvinceController(IRepository<Province, Guid> rep, IUnitOfWork<ApplicationDbContext> uow)
    {
        _countryRep = rep;
        _uow = uow;
    }

    [HttpGet()]
    [Consumes("application/json")]
    public async Task<ActionResult<List<Province>>> GetAll()
    {
        var country = await _countryRep.GetAll().Include(el => el.Country)
            .Select(el => new
            {
                el.Id,
                el.Title,
                Country = new {el.Country.Title, el.Country.Id}
            }).ToListAsync();
        return Ok(country);
    }

    [HttpGet("{provinceId:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<Province>> Get(Guid provinceId,  CancellationToken ct = default)
    {
        var province = await _countryRep.GetByIdAsync(provinceId, ct);
        return Ok(province);//ToDo make a wrapper
    }
    
    [HttpPost()]
    [Consumes("application/json")]
    public async Task<ActionResult<Province>> Post([FromBody] ProvinceDto newProvince, CancellationToken ct = default)
    {
        using (_uow)
        {
            var province = newProvince.Adapt<Province>();
            await _countryRep.AddAsync(province, ct);
            await _uow.SaveChangesAsync(ct);
            return Ok(province);
        }
    }
}