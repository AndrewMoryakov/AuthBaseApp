using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Data.Entities.Service;

public class UserRoot: IdentityUser, ITrackable 
{
    public byte[] RowVersion { get; set; }
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
}