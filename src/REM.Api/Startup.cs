using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using REM.Api.Configuration;
using REM.Api.Data;
using REM.Api.Data.Models;
using REM.Api.Middleware;
using System.Linq;

namespace REM.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<RemDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<RemDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // this will do the initial DB population
            InitializeDatabase(app);

            app.UseRemAuthentication(options: new RemAuthenticationOptions()
            {
                EnforceHashValidation = false
            });


            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var remDbContext = serviceScope.ServiceProvider.GetRequiredService<RemDbContext>();

                remDbContext.Database.Migrate();

                if (!remDbContext.Users.Any())
                {
                    foreach (var user in Users.Get())
                    {
                        remDbContext.Users.Add(user);
                    }
                    remDbContext.SaveChanges();
                }
            }
        }
    }
}
