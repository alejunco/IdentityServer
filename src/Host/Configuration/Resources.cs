using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Host.Configuration
{
    public class Resources
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("api1", "Asp.Net Core Web API")
                {
//                    Name = "api1",
//                    DisplayName = "Asp.Net Core Web API",

                    // this is needed for introspection when using reference tokens
                    ApiSecrets = {new Secret("secret".Sha256())},

//                    Scopes =
//                    {
//                        new Scope()
//                        {
//                            Name = "api1.full_access",
//                            DisplayName = "Full access to Asp.Net Core Web API"
//                        },
//                        new Scope
//                        {
//                            Name = "api1.read_only",
//                            DisplayName = "Read only access to Asp.Net Core Web API"
//                        }
//                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Phone(),
                new IdentityResources.Email()
//                new IdentityResources.
            };
        }
    }
}
