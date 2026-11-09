using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;
        public ParticipantController(ApiService apiService) { _apiService = apiService; }

        public async Task<IActionResult> MyEnrolments()
        {

            var enrolments = await _apiService.GetAsync<List<object>>("me/enrolments");
            return View(enrolments ?? new List<object>());
        }
    }
}