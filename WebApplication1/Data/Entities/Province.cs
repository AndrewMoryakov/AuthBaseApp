using WebApplication1.Data.Entities.Service;

namespace WebApplication1.Data.Entities;

public class Province : Entity<Guid>
{
        public string Title { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; }
}