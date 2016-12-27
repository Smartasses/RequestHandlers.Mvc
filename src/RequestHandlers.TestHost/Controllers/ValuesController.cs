using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RequestHandlers.TestHost.Controllers
{
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet("api/Values")]
        public IEnumerable<string> Get([Microsoft.AspNetCore.Mvc.FromQuery()]string test)
        {
            return new string[] { "value1", "value2" };
        }
    }
}
