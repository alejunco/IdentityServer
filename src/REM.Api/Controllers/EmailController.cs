using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace REM.Api.Controllers
{
    [Route("api/email")]
    public class EmailController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "email1", "email2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "email 1";
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]EmailDto email)
        {
            var user = HttpContext.User;
            var authenticationMethod = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod)?.Value;

            return Ok($"email from {email.Phone} Posted. Using Authentication Method: {authenticationMethod}");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class EmailDto
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string DeviceId { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// Phone Detection Method Reference
        /// </summary>
        public string Pdmr { get; set; }
        /// <summary>
        /// Hashed Secret DeviceId + Phone + Subject
        /// </summary>
        public string Secret { get; set; }
    }


}
