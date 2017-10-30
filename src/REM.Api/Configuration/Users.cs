using System.Collections.Generic;


namespace REM.Api.Configuration
{
    public class Users
    {
        public static IEnumerable<ApplicationUser> Get()
        {
            return new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "17863007263",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                    PhoneNumber = "17863007263",
                    PhoneNumberConfirmed = true,
                    Email = "ale911115@gmail.com",
                    EmailConfirmed = true,
                }
            };
        }
    }
}
