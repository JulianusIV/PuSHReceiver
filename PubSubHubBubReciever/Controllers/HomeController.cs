using Contracts.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.Models;
using System.Diagnostics;

namespace PubSubHubBubReciever.Controllers
{
    public class HomeController : Controller
    {
        private readonly IShutdownService _shutdownService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public HomeController(IShutdownService shutdownService, IHostApplicationLifetime applicationLifetime)
        {
            _shutdownService = shutdownService;
            _applicationLifetime = applicationLifetime;
        }

        public IActionResult Index()
            => View();

        public IActionResult Privacy()
            => View();

        [Authorize("AdminOnly")]
        public IActionResult Shutdown()
        {
            _shutdownService.Shutdown();
            _applicationLifetime.StopApplication();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
