using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    public class ValueManager : IValueService
    {
        public async Task<IEnumerable<string>> Get()
        {
            var values = new List<string>()
            {
                "value1",
                "value2",
                "value3"
            };

            return values;
        }
    }
}
