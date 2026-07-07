using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPO_Workflow.ViewModels;
using System.Diagnostics;
using NPO_Workflow.DAL;
namespace NPO_Workflow.Controllers
{
    // [Authorize(Roles = @"DESKTOP-RDH7AFR\docker-users")]
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices] NPOContext context)
        {
            var query = (from te in context.Technologies where te.BeginDate == null || te.EndDate == null
                select te).Count();     
            ViewBag.Count = query;
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
