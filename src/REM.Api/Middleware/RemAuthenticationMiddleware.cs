using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PhoneNumbers;
using REM.Api.Controllers;
using REM.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace REM.Api.Middleware
{

    public class RemAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RemAuthenticationOptions _options;

        public RemAuthenticationMiddleware(RequestDelegate next, IOptions<RemAuthenticationOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            if (options == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_options.Paths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                var injectedRequestStream = new MemoryStream();
                try
                {
                    using (var bodyReader = new StreamReader(context.Request.Body))
                    {
                        #region Build EmailDto from Request.Body

                        var jsonData = bodyReader.ReadToEnd();

                        var emailDto = JsonConvert.DeserializeObject<EmailDto>(jsonData);

                        if (emailDto == null)
                        {
                            context.Response.StatusCode = 400;
                            return;
                        }


                        #endregion

                        #region Validate DeviceId, Phone and Secret fields aren't null or Empty

                        if (string.IsNullOrWhiteSpace(emailDto.DeviceId) || string.IsNullOrWhiteSpace(emailDto.Phone) || (_options.EnforceHashValidation && string.IsNullOrEmpty(emailDto.Secret)))
                        {
                            context.Response.StatusCode = 404;
                            return;
                        }

                        #endregion

                        #region Validate Hash

                        if (_options.EnforceHashValidation)
                        {
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
                                context.Response.StatusCode = 401;
                                return;
                            }
                        }

                        #endregion

                        #region Build valid PhoneNumber object from request

                        var phoneUtil = PhoneNumberUtil.GetInstance();
                        var phone = phoneUtil.Parse("+" + emailDto.Phone, "");

                        if (phone == null)
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }

                        #endregion

                        #region Validate Phone is from a Restricted Country

                        if (_options.RestrictedCountryCodes.All(code => code != phone.CountryCode))
                        {
                            context.Response.StatusCode = 403;
                            return;
                        }

                        #endregion

                        #region Check if exist user with the validated phone and create new user if doesn't exist

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

                        #endregion

                        #region Add ClaimsPricipal to Http Context

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

                        //New Feature in the development branch

                        // My second new feature

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