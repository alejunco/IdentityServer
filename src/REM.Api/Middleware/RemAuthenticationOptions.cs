using System.Collections.Generic;

namespace REM.Api.Middleware
{
    /// <summary>
    /// RemAuthentication Middleware Configurable Options
    /// </summary>
    public class RemAuthenticationOptions
    {
        /// <summary>
        /// Collection of endpoint paths to apply RemAuthentication
        /// Default endpoint path is "/api/email"
        /// </summary>
        public IEnumerable<string> Paths { get; set; } = new[] { "/api/email" };

        /// <summary>
        /// Collection of Restricted Country Codes for using
        /// the insecure endpoint
        /// Default values are {53, 233} Cuba & Ghana
        /// </summary>
        public IEnumerable<int> RestrictedCountryCodes { get; set; } = new[] { 53, 233 };

        /// <summary>
        /// Sets wether a hash validation has to be enforced
        /// Default value is true
        /// </summary>
        public bool EnforceHashValidation { get; set; } = true;
    }
}