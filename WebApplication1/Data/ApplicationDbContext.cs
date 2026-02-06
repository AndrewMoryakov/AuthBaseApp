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
    public DbSet<Province> Provinces { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");

        builder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.Property(x => x.UserId).HasMaxLength(128);
            b.Property(x => x.TokenHash).HasMaxLength(88);
        });
    }
}
