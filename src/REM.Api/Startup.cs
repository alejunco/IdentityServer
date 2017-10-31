using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PhoneNumbers;
using REM.Api.Configuration;
using REM.Api.Controllers;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
//                .AddUserManager<ApplicationUser>()
                .AddEntityFrameworkStores<RemDbContext>()
                .AddDefaultTokenProviders();

//            services
//                .AddAuthentication(options => { options.DefaultScheme = "rem"; });

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

//            app.UseAuthentication();

            app.UseRemAuthentication();

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

    //
    // Summary:
    //     Extension methods to add authentication capabilities to an HTTP application pipeline.
    public static class RemAuthAppBuilderExtensions
    {
        //
        // Summary:
        //     Adds the RemAuthenticationMiddleware to the
        //     specified Microsoft.AspNetCore.Builder.IApplicationBuilder, which enables authentication
        //     capabilities for Restricted Countries.
        //
        // Parameters:
        //   app:
        //     The Microsoft.AspNetCore.Builder.IApplicationBuilder to add the middleware to.
        //
        // Returns:
        //     A reference to this instance after the operation has completed.
        public static IApplicationBuilder UseRemAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RemAuthenticationMiddleware>();
        }
    }


    public class RemAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public IAuthenticationSchemeProvider Schemes { get; set; }

        public RemAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (schemes == null)
            {
                throw new ArgumentNullException(nameof(schemes));
            }

            _next = next;
            Schemes = schemes;
        }

        public async Task Invoke(HttpContext context)
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
                        {
                            context.Response.StatusCode = 400;
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(emailDto.DeviceId) || string.IsNullOrWhiteSpace(emailDto.Phone) || string.IsNullOrEmpty(emailDto.Secret))
                        {
                            context.Response.StatusCode = 404;
                            return;
                        }

                        // validate hash
                        var hash = "";
                        using (var md5 = MD5.Create())
                        {
                            var result =
                                md5.ComputeHash(
                                    Encoding.ASCII.GetBytes(emailDto.DeviceId + emailDto.Phone + emailDto.Subject));
                            hash = Encoding.ASCII.GetString(result);
                        }

                        if (hash != emailDto.Secret)
                        {
                            //if not -> Log and respond forbidden 403
                            context.Response.StatusCode = 403;
                            return;
                        }
                        var phoneUtil = PhoneNumberUtil.GetInstance();
                        var phone = phoneUtil.Parse("+" + emailDto.Phone, "");

                        if (phone == null)
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }

                        //validate phone is from a restricted country
                        //if not -> forbidden 403    
                        if (RestrictedCountryCodes.Get().All(code => code != phone.CountryCode))
                        {
                            context.Response.StatusCode = 403;
                            return;
                        }
                        //check if phone already exist
                        //if not -> create new account

                        var userManager = context.RequestServices
                            .GetService<UserManager<ApplicationUser>>();

                        var user = await userManager.FindByNameAsync(emailDto.Phone);

                        if (user == null)
                        {
                            user = new ApplicationUser()
                            {
                                UserName = emailDto.Phone,
                                PhoneNumber = emailDto.Phone
                            };


                            if (emailDto.Pdmr.ToLower() == Constants.RemAuth.Pdmr.Automatic)
                                user.PhoneNumberConfirmed = true;

                            await userManager.CreateAsync(user);

                            await userManager.AddClaimAsync(user, new Claim("Name", emailDto.Phone));

                            await userManager.AddClaimAsync(user,
                                new Claim(Constants.RemAuth.ClaimTypes.DeviceId, emailDto.DeviceId));
                        }
                        else
                        {
                            var userClaims = await userManager.GetClaimsAsync(user);

                            var deviceId = userClaims.FirstOrDefault(claim =>
                                    claim.Type == Constants.RemAuth.ClaimTypes.DeviceId)
                                ?.Value;
                        }

                        //SignIn User

                        #region SignIn Method One Using SignInManager

//                                            var signInManager = context.RequestServices.GetService<SignInManager<ApplicationUser>>();
//                                            
//                                            await signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: "hash");

                        #endregion

                        #region SignIn Method Two Using HttpContext SignIn Method

                        var userClaimssList = await userManager.GetClaimsAsync(user);

                        var identity = new ClaimsIdentity(userClaimssList, "hash");

                        identity.AddClaims(new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                            new Claim(ClaimTypes.AuthenticationMethod, identity.AuthenticationType)
                        });

                        context.User = new ClaimsPrincipal(identity);

                        #endregion

                        #region Rewrite Body Stream

                        var bytesToWrite = Encoding.UTF8.GetBytes(jsonData);
                        injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        injectedRequestStream.Seek(0, SeekOrigin.Begin);
                        context.Request.Body = injectedRequestStream;

                        #endregion

                        // Call the next delegate / middleware in the pipeline
                        await this._next(context);
                    }
                }
                finally
                {
                    injectedRequestStream.Dispose();
                }
            }
            else
            {
                // Call the next delegate / middleware in the pipeline
                await this._next(context);
            }
        }
    }
}
