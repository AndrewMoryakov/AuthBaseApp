using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data.Entities;
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

        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddEntityFrameworkSqlite().AddDbContext<ApplicationDbContext>(options =>
		        options.UseSqlite(Configuration.GetConnectionString("DefaultConnectionSqllite")));
	        
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
	        })
		        .AddJwtBearer(options =>
		        {
			        options.SaveToken = true;
			        options.RequireHttpsMetadata = false;
			        options.TokenValidationParameters = new TokenValidationParameters
			        {
				        ValidateIssuer = true,
				        ValidIssuer = AuthOptions.Issuer,
				        ValidateAudience = true,
				        ValidAudience = AuthOptions.Audience,
				        ValidateLifetime = true,
				        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
				        ValidateIssuerSigningKey = true,
			        };
		        });
        }
        
        private static void AddRepositories(IServiceCollection services)
        {
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
                c.DefaultModelsExpandDepth(0);
                c.RoutePrefix = string.Empty;
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
