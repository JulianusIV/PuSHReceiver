using Contracts;
using Contracts.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using PluginLibrary.Interfaces;
using System.Security.Claims;

namespace PuSHReceiver.Controllers
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
            => RedirectToAction("Dashboard", "Subscriptions");

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

        [HttpPost]
        public IActionResult Create(Lease lease)
        {
            lease.Owner = _userRepo.GetUser(Convert.ToInt32(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value));

            _leaseRepo.CreateLease(lease);

            if (lease.Active)
                _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease);

            return RedirectToAction("Dashboard", "Subscriptions");
        }

        public IActionResult Edit(int id)
        {
            var publisherNames = _pluginManager.GetPublisherNames();
            var consumerNames = _pluginManager.GetConsumerNames();

            ViewBag.Publishers = publisherNames;
            ViewBag.Consumers = consumerNames;

            var lease = _leaseRepo.FindLease(id);

            return View(lease);
        }

        [HttpPost]
        public IActionResult Edit(Lease lease)
        {
            var before = _leaseRepo.FindLease(lease.Id);
            _leaseRepo.UpdateLease(lease);

            if (before!.Active != lease.Active)
                _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease, lease.Active);

            return RedirectToAction("Dashboard", "Subscriptions");
        }

        public IActionResult Details(int id)
        {
            var lease = _leaseRepo.FindLease(id);

            return View(lease);
        }

        public IActionResult Delete(int id)
        {
            var lease = _leaseRepo.FindLease(id)!;
            if (lease.Active)
                _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease, false);

            _leaseRepo.DeleteLease(id);

            return RedirectToAction("Dashboard", "Subscriptions");
        }
    }
}
