using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private string path;
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationUser> AspNetUsers { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Country> Provinces { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
    }
}



public class Country
{
    public int Id { get; set; }
    public string Title { get; set; }
    public virtual ICollection<Province> Provinces { get; set; }
}

public class Province
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
}