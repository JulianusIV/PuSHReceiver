using Contracts;
using Contracts.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using System.Security.Claims;
using System.Security.Policy;

namespace PubSubHubBubReciever.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly ILeaseRepository _leaseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IPluginManager _pluginManager;

        public SubscriptionsController(ILeaseRepository leaseRepo, IUserRepository userRepo, IPluginManager pluginManager)
        {
            _leaseRepo = leaseRepo;
            _userRepo = userRepo;
            _pluginManager = pluginManager;
        }

        public IActionResult Index()
            => RedirectToAction("Subscriptions", "Dashboard");

        public IActionResult Dashboard()
        {
            var user = _userRepo.GetUser(Convert.ToInt32(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value));

            var leases = _leaseRepo.GetLeasesByUser(user);

            return View(leases.ToArray());
        }

        public IActionResult Create()
        {
            var publisherNames = _pluginManager.GetPublisherNames();
            var consumerNames = _pluginManager.GetConsumerNames();

            ViewBag.Publishers = publisherNames;
            ViewBag.Consumers = consumerNames;

            return View(new Lease("Subscription1", @"https://www.youtube.com/xml/feeds/videos.xml?channel_id=my_channel_id"));
        }

        public IActionResult Edit(int id)
        {
            throw new NotImplementedException();
        }

        public IActionResult Details(int id)
        {
            throw new NotImplementedException();
        }

        public IActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
