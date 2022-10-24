using Microsoft.AspNetCore.Identity;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Repositories;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace WebApplication1.Data;

public class UserStore : IUserStore
{
    private ApplicationDbContext _context;
    private IPasswordHasher<ApplicationUser> _pswrdHasher;
    private IUserRepository _userRep;
    private IUnitOfWork<ApplicationDbContext> _uow;

    public UserStore(ApplicationDbContext context, IPasswordHasher<ApplicationUser> pswHasher,
        IUserRepository usrRep, IUnitOfWork<ApplicationDbContext> uow)
    {
        _context = context;
        _pswrdHasher = pswHasher;
        _userRep = usrRep;
        _uow = uow;
    }

    public IList<ApplicationUser> Get()
    {
        using (_uow)
        {
            return _userRep.GetAll().ToList();
        }
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, string password, CancellationToken ct)
    {
        using (_uow)
        {
            user.NormalizedEmail = user.Email.ToUpper();
            if (!_context.Users.Any(u => u.NormalizedEmail == user.NormalizedEmail))
            {
                user.CreatedDateTime = DateTimeOffset.Now;
                var hashed = _pswrdHasher.HashPassword(user, password);
                user.PasswordHash = hashed;
                await _userRep.AddAsync(user, ct);
                await _uow.SaveChangesAsync(ct);
            }
            else
            {
                throw new BadHttpRequestException("User with email already exists");
            }
        }

        return user;
    }
}