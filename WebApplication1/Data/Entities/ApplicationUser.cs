using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Entities;

public class ApplicationUser : UserRoot<string>
{
    public virtual ICollection<Country> Countries { get; set; }
}