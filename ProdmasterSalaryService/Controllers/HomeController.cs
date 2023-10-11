using Microsoft.AspNetCore.Mvc;
using ProdmasterSalaryService.Models;
using System.Diagnostics;

namespace ProdmasterSalaryService.Controllers
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
            return RedirectToAction(nameof(Index), nameof(ReportController).Replace("Controller", ""));
        }

        public IActionResult Privacy()
        {
            return RedirectToAction(nameof(Index), nameof(ReportController).Replace("Controller", ""));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}