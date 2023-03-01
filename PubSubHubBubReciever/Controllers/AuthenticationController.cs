using Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.Models;

namespace PubSubHubBubReciever.Controllers
{
    public class AuthenticationController : Controller
    {
        IUserRepository _userRepository;

        public AuthenticationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            _userRepository.SignOut(HttpContext);
            return RedirectToAction("Login", "Authentication");
        }

        [HttpPost]
        public IActionResult Login(LoginModel form)
        {
            if (!ModelState.IsValid)
                return View(form);
            try
            {
                _userRepository.SignIn(HttpContext, form.Username, form.Password);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("summary", ex.Message);
                return View(form);
            }
        }
    }
}
