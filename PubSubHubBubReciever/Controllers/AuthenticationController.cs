using Contracts.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.Models;

namespace PubSubHubBubReciever.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Subscriptions");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        [HttpPost]
        public IActionResult Login(LoginModel form)
        {
            if (!ModelState.IsValid)
                return View(form);
            try
            {
                var claims = _userRepository.GetUserClaims(form.Username, form.Password);
                if (claims is null)
                    return View(form);

                HttpContext.SignInAsync(claims).Wait();
                return RedirectToAction("Dashboard", "Subscriptions");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("summary", ex.Message);
                return View(form);
            }
        }
    }
}
