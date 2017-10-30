using System.Collections.Generic;

namespace REM.Api.Configuration
{
    public class RestrictedCountryCodes
    {
        public static IEnumerable<int> Get()
        {
            return new List<int>()
            {
                53
            };
        }
    }
}
