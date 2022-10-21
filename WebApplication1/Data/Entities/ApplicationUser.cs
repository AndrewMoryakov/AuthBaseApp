using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Entities;

public class ApplicationUser : UserRoot
{
    public virtual IList<Country> Countries { get; set; }
}