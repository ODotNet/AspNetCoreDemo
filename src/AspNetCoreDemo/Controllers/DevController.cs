using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AspNetCoreDemo.Controllers
{
    [Route("[controller]")]
    public class DevController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]"), AllowAnonymous, HttpGet, HttpHead]
        public IActionResult Test()
        {
            return Content("0");
        }
    }
}
