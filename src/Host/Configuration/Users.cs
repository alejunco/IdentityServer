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
                        new Claim("name", "Alex")
                    }
                }
            };
        }
    }
}
