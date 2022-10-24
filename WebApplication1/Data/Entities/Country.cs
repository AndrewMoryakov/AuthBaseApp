using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Entities;

public class Country : Entity<Guid>
{
    public string Title { get; set; }
    public virtual ICollection<Province> Provinces { get; set; }
}