using IdentityServer4.Models;
using System.Collections.Generic;

namespace Host.Configuration
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>()
            {
                new Client
                {
                    ClientId = "client",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = {"openid", "webApi" }
                },
                new Client
                {
                    ClientId = "mvc.hybrid",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = false,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // where to redirect to after login
                    RedirectUris =
                    {
                        "http://localhost:5002/signin-oidc"
                    },

                    // where to redirect to after logout
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:5002/signout-callback-oidc"
                    },

                    AllowOfflineAccess = true,
                    AlwaysSendClientClaims = true,
                    // scopes that client has access to
                    AllowedScopes =
                    {
                        "api1",
                        IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
                        IdentityServer4.IdentityServerConstants.StandardScopes.Phone,
                        IdentityServer4.IdentityServerConstants.StandardScopes.Email
                        
                    }
                    
                    
                }
            };
        }
    }
}
