using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YatriSewa.Controllers
{
    [Authorize(Roles = "Passenger")]
    public class PassengerController : Controller
    {
        public IActionResult PassengerDashboard()
        {
            return View();
        }
    }
}
