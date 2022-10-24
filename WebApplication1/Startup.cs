using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Entities.Service;
using WebApplication1.Data.Repositories;
using WebApplication1.Security;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services )
        {
	        services.AddSwaggerGen(c =>
	        {
		        c.SwaggerDoc("v1", new OpenApiInfo {Title = "Base auth api", Version = "v1"});
		        c.EnableAnnotations();
	        });

	        services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
	        services.AddScoped<IUnitOfWork<ApplicationDbContext>, UnitOfWork>();
	        InjectRepositories(services);
	        services.AddScoped<IStoreOfUsers, StoreOfUsers>();
	        
	        services.AddControllers(configure =>
	        {
		        //configure.Filters.Add(typeof(ExceptionFilter));//ToDo
	        });
	        services.AddDbContext<ApplicationDbContext>(options =>
		           options.UseSqlite($"Data Source={AppContext.BaseDirectory}/data.db"));
	        
	        services.AddIdentity<ApplicationUser, IdentityRole>(p
		        =>
		        {
			        p.Password.RequireDigit = true;
			        p.Password.RequiredLength = 6;
			        p.Password.RequireLowercase = true;
			        p.Password.RequireUppercase = true;
		        })
		        .AddEntityFrameworkStores<ApplicationDbContext>()
		        .AddDefaultTokenProviders();

	        services.AddAuthentication(options =>
	        {
		        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	        });
	        services.AddScoped<ITokenFactory<ApplicationUser>, TokenFactory<ApplicationUser>>();	        
	        services.AddScoped<IAuthenticationService<ApplicationUser>, AuthenticationService<ApplicationUser>>();
	        
	        
	        services.Configure<AuthTokenOptions>(Configuration.GetSection("AuthOptions"));
	        var authTokenOptions = Configuration.GetSection("AuthOptions").Get<AuthTokenOptions>() as AuthTokenOptions;
	        var signingConfigurations = new SigningCredentialsKeys(authTokenOptions.Secret) as SigningCredentialsKeys;
	        services.AddSingleton(signingConfigurations);
	        services.ConfigureJwtAuthentication(authTokenOptions, signingConfigurations);

	        services.AddAuthorization();
        }
        
        private void InjectRepositories(IServiceCollection services)
        {
	        services.AddScoped<IUserRepository, UserRep>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mock JWT Auth API");
                c.DocumentTitle = "test api";
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
