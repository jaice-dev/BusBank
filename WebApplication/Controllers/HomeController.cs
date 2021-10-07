using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpPost]
        public IActionResult Search(SearchPostCodeModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("test is valid");
                ViewBag.ValidSearch = true;
                return View("Results");
            }
            
            _logger.LogInformation(model.Target);

            ViewBag.Target = model.Target ?? "nav";
            ViewBag.ErrorMessage = $"Value '{model.PostCode}' is not a valid postcode.";
            ViewBag.ValidSearch = false;
            return View("Index");
        }
    }
}