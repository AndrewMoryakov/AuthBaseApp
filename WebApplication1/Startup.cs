using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Data.Entities;
using WebApplication1.Data.Entities.Service;
using WebApplication1.Data.Repositories;
using WebApplication1.Filters;
using WebApplication1.Security;
using WebApplication1.Services;

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

	        services.Configure<GlobalExceptionFilterOptions>(options =>
	        {
		        options.DetailLevel = ExceptionDetailLevel.Message;
	        });
	        services.AddControllers(configure =>
	        {
		        configure.Filters.Add(typeof(ExceptionFilter));
	        });
	        services.AddDbContext<ApplicationDbContext>(options =>
		           options.UseSqlite(Configuration.GetConnectionString("DefaultConnectionSqlite")
		                             ?? $"Data Source={AppContext.BaseDirectory}/data.db"));
	        
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

                // External login cookie used by OAuth flows (Google). Keep it compatible with local HTTP dev.
                services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
                {
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

                services.AddScoped<IJwtTokenService, JwtTokenService>();
                services.AddScoped<IAuthenticationService<ApplicationUser>, AuthenticationService<ApplicationUser>>();
                services.AddScoped<IRefreshTokenService, RefreshTokenService>();
                services.Configure<ServiceClientOptions>(Configuration.GetSection("ServiceClients"));
                services.AddHttpClient<ILlmClient, OpenRouterClient>();
	        
	        
	        services.Configure<AuthTokenOptions>(Configuration.GetSection("AuthOptions"));
	        var authTokenOptions = Configuration.GetSection("AuthOptions").Get<AuthTokenOptions>() as AuthTokenOptions;
	        var signingConfigurations = new SigningCredentialsKeys(authTokenOptions.Secret) as SigningCredentialsKeys;
	        services.AddSingleton(signingConfigurations);
	        services.ConfigureJwtAuthentication(authTokenOptions, signingConfigurations);

                // Optional: Google OAuth (external login). Configure Authentication:Google:ClientId/ClientSecret.
                var googleClientId = Configuration["Authentication:Google:ClientId"];
                var googleClientSecret = Configuration["Authentication:Google:ClientSecret"];
                if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
                {
                    services.AddAuthentication()
                        .AddGoogle("Google", options =>
                        {
                            options.ClientId = googleClientId;
                            options.ClientSecret = googleClientSecret;
                            options.SignInScheme = IdentityConstants.ExternalScheme;
                            options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        });
                }

	        services.AddAuthorization();

                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", policy =>
                    {
                        policy
                            .WithOrigins("http://localhost:5173")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                });
        }
        
        private void InjectRepositories(IServiceCollection services)
        {
	        services.AddScoped<IUserRepository, UserRep>();
	        services.AddScoped<IRepository<Province, Guid>, Repository<Province, Guid>>();
	        services.AddScoped<IRepository<Country, Guid>, Repository<Country, Guid>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (!env.IsEnvironment("Testing"))
            {
                app.UseHttpsRedirection();
            }

            // Apply EF Core migrations at startup (dev-friendly). In production you may prefer out-of-band migration.
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mock JWT Auth API");
                c.DocumentTitle = "test api";
            });

            app.UseRouting();
            // CORS must run between routing and auth so that 401/403 responses still contain CORS headers.
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
