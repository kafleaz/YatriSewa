using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YatriSewa.Models;
using System.Linq;
using System.Threading.Tasks;

namespace YatriSewa.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly ApplicationContext _context;

        public DriverController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult DriverDashboard()
        {
            //// Fetch data for recent journeys
            //var recentJourneys = _context.Journeys
            //    .OrderByDescending(j => j.Date)
            //    .Take(5) // Fetch top 5 recent journeys
            //    .ToList();

            // Fetch passenger list data
            var passengers = _context.User_Table
                .Where(u => u.Role == UserRole.Passenger) // Assuming "Role" is an enum
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.PhoneNo
                })
                .ToList();

            // Pass data to the view using a dynamic ViewModel
            var viewModel = new
            {
                //RecentJourneys = recentJourneys,
                Passengers = passengers
            };

            return View(viewModel);
        }

        public IActionResult Journey()
        {
            return View();
        }

        public IActionResult PassengerList()
        {
            // Assuming PassengerList is now integrated into the DriverDashboard
            return RedirectToAction("DriverDashboard");
        }
    }
}