using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LemonAuction.Identity;
using Microsoft.AspNetCore.Identity;

namespace LemonAuction.Controllers
{
    public class HomeController : Controller
    {

        // private readonly UserManager<LemonUser> _userManager;
        // private readonly SignInManager<LemonUser> _signInManager;


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
