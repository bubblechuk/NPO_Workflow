using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPO_Workflow.ViewModels;
using System.Diagnostics;

namespace NPO_Workflow.Controllers
{
    [Authorize(Roles = @"DESKTOP-RDH7AFR\docker-users")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
