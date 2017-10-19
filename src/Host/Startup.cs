using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Host.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryApiResources(Resources.GetApiResources());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerfactory.AddSerilog();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseIdentityServer();
        }
    }
}
