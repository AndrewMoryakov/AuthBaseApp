using System.Security.Principal;

namespace WebApplication1.Data.Entities.Service;

public interface IHasKey<T> 
{
    T Id { get; set; }
}