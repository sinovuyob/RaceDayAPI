using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in and is an Organiser
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser") return RedirectToAction("Login", "Account");

            // Fetch events
            var events = await _apiService.GetAsync<List<EventResponseDto>>("events");
            return View(events ?? new List<EventResponseDto>());
        }
    }
}