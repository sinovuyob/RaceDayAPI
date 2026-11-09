using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Diagnostics;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch all events from the API
            var events = await _apiService.GetAsync<List<EventResponseDto>>("events");

            // If the API is down or returns null, pass an empty list
            return View(events ?? new List<EventResponseDto>());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}