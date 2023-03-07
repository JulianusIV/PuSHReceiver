using Contracts.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuSHReceiver.Models;
using System.Security.Claims;

namespace PuSHReceiver.Controllers
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

        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        [Authorize]
        [HttpPost]
        public IActionResult Settings(UserModel model)
        {
            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword) && model.Password == model.ConfirmPassword)
            {
                _userRepository.UpdateUser(model.Id, model.Username, model.Password);
                
                HttpContext.SignOutAsync();
                var claims = _userRepository.GetUserClaims(model.Username, model.Password);
                HttpContext.SignInAsync(claims!);
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Settings() 
        {
            var user = _userRepository.GetUser(Convert.ToInt32(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value));

            var userModel = new UserModel(user.UserName) { Id = user.Id };

            return View(userModel);
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
