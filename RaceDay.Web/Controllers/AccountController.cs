using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync<LoginViewModel, AuthResponse>("auth/login", model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Save token and user info in Session
                HttpContext.Session.SetString("JWToken", response.Token);
                HttpContext.Session.SetString("UserRole", response.User.Role);
                HttpContext.Session.SetString("UserName", response.User.FullName);

                if (response.User.Role == "Organiser")
                    return RedirectToAction("Index", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync<RegisterViewModel, AuthResponse>("auth/register", model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Automatically log them in after registration
                HttpContext.Session.SetString("JWToken", response.Token);
                HttpContext.Session.SetString("UserRole", response.User.Role);
                HttpContext.Session.SetString("UserName", response.User.FullName);

                if (response.User.Role == "Organiser")
                    return RedirectToAction("Index", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Registration failed. Email might already be in use.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}