using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.Models;
using System.Diagnostics;

namespace PubSubHubBubReciever.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
            => View();

        public IActionResult Privacy()
            => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
