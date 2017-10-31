using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace REM.Api.Middleware
{
    //
    // Summary:
    //     Extension methods to add authentication capabilities to an HTTP application pipeline.
    public static class RemAuthenticationAppBuilderExtensions
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

        public static IApplicationBuilder UseRemAuthentication(this IApplicationBuilder app, RemAuthenticationOptions options)
        {
            return app.UseMiddleware<RemAuthenticationMiddleware>(Options.Create(options));
        }
    }
}