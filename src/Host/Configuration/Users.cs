using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Test;

namespace Host.Configuration
{
    public class TestUsers
    {
        public static List<TestUser> Get()
        {
            return new List<TestUser>()
            {
                new TestUser()
                {
                    SubjectId = "368574d4-65cb-44c9-8f4f-47a0b0d04471",
                    Username = "bob",
                    Password = "password",
                    IsActive = true,
                    Claims = new []
                    {
                        new Claim(IdentityModel.JwtClaimTypes.Name, "Alejandro"),
                        new Claim(IdentityModel.JwtClaimTypes.FamilyName, "Junco"),
                        new Claim(IdentityModel.JwtClaimTypes.PhoneNumber, "+1 (786) 300-7263"),
                        new Claim(IdentityModel.JwtClaimTypes.PhoneNumberVerified, "true"), 
                        new Claim(IdentityModel.JwtClaimTypes.Email, "ale911115@gmail.com"),
                        new Claim(IdentityModel.JwtClaimTypes.EmailVerified, "true"),
                        new Claim(IdentityModel.JwtClaimTypes.BirthDate, "1991-11-15")

                    }
                }
            };
        }
    }
}
