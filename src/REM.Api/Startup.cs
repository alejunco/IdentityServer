using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using REM.Api.Controllers;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using PhoneNumbers;
using REM.Api.Configuration;

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

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    
                })
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

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/email"))
                {
                    var injectedRequestStream = new MemoryStream();
                    try
                    {
                        using (var bodyReader = new StreamReader(context.Request.Body))
                        {
                            var jsonData = bodyReader.ReadToEnd();

                            var emailDto = JsonConvert.DeserializeObject<EmailDto>(jsonData);

                            if (emailDto == null)
                                context.Response.StatusCode = 400;

                            else
                            {
                                if (string.IsNullOrWhiteSpace(emailDto.DeviceId) ||
                                    string.IsNullOrWhiteSpace(emailDto.Phone))
                                {
                                    context.Response.StatusCode = 401;
                                }
                                else
                                {
                                    //TODO validate hash
                                    //if not -> Log and respond forbidden 403

                                    var phoneUtil = PhoneNumberUtil.GetInstance();
                                    var phone = phoneUtil.Parse("+" + emailDto.Phone, "");

                                    if (phone==null)
                                        context.Response.StatusCode = 401;
                                    else
                                    {
                                        //validate phone is from a restricted country
                                        //if not -> forbidden 403    
                                        if (RestrictedCountryCodes.Get().All(code => code != phone.CountryCode))
                                            context.Response.StatusCode = 403;
                                        else
                                        {
                                            //check if phone already exist
                                            //if not -> create new account

                                            var remDbContext = context.RequestServices.GetService<RemDbContext>();
                                            var user = await remDbContext.Users.FirstOrDefaultAsync(x => x.UserName == emailDto.Phone);
                                            if (user == null)
                                            {
                                                user = new ApplicationUser()
                                                {
                                                    UserName = emailDto.Phone,
                                                    PhoneNumber = emailDto.Phone,
                                                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret")
                                                };

                                                remDbContext.Add(user);

                                                await remDbContext.SaveChangesAsync();
                                            }

                                            //TODO add user to principal
//                                            context.SignInAsync("", new ClaimsPrincipal())

                                            var bytesToWrite = Encoding.UTF8.GetBytes(jsonData);
                                            injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                                            injectedRequestStream.Seek(0, SeekOrigin.Begin);
                                            context.Request.Body = injectedRequestStream;

                                            await next();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        injectedRequestStream.Dispose();
                    }
                }
                else
                {
                    await next();
                }
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

    public class RemDbContext : IdentityDbContext<ApplicationUser>
    {
        public RemDbContext(DbContextOptions<RemDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }

    public class ApplicationUser : IdentityUser
    {
    }
}
