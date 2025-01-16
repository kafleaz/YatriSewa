using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YatriSewa.Services;
using YatriSewa.Services.Interfaces;

namespace YatriSewa.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        public IActionResult DriverDashboard()
        {
            return View();
        }

        public IActionResult BusDetails()
        {
            var busDetails = _driverService.GetBusDetails();
            return View(busDetails); // Ensure a corresponding view is created
        }

        public IActionResult PassengerList(string tripDate)
        {
            var passengerList = _driverService.GetPassengerList(tripDate);
            return View(passengerList); // Ensure a corresponding view is created
        }

        public IActionResult Routes()
        {
            var routes = _driverService.GetRoutes();
            return View(routes); // Ensure a corresponding view is created
        }

        public IActionResult TrackLocation()
        {
            var location = _driverService.GetCurrentLocation();
            return Json(location); // Returning JSON for location data visualization
        }
    }
}
