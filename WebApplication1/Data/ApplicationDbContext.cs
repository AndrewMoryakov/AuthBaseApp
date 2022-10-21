using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
}



public class Country
{
    public int Id { get; set; }
    public string Title { get; set; }
    public virtual IList<Province> Provinces { get; set; }
}

public class Province
{
    public int Id { get; set; }
    public string Title { get; set; }
}