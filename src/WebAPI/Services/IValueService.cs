using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    public interface IValueService
    {
        Task<IEnumerable<string>> Get();
    }
}
