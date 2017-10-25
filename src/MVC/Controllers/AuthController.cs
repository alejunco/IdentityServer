using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace MVC.Controllers
{
    [Route("auth")]
    public class AuthController:Controller
    {
        [Route("signout")]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
        [Route("signin")]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties() {RedirectUri = "/"});
        }
    }
}
