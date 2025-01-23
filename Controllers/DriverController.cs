using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using YatriSewa.Models;
using YatriSewa.Services;

namespace YatriSewa.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        private readonly ApplicationContext _context;

        public DriverController(IDriverService driverService, ApplicationContext context)
        {
            _driverService = driverService;
            _context = context;
        }

        // Dashboard: Display schedules for the logged-in driver
        public async Task<IActionResult> DriverDashboard(DateTime? date)
        {
            // Get the UserId of the logged-in user from claims
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // Fetch DriverId using UserId
            var driver = await _context.Driver_Table.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null)
            {
                return Unauthorized("You are not assigned as a driver.");
            }

            var driverId = driver.DriverId; // Correct DriverId from the Drivers table

            // Use only the date part
            var selectedDate = date?.Date ?? DateTime.Today.Date;

            // Fetch schedules for the driver
            var schedules = await _driverService.GetSchedulesForDriverAsync(driverId, selectedDate);

            // Debugging: Log details
            Console.WriteLine($"UserId: {userId}, DriverId: {driverId}, SelectedDate: {selectedDate}, Schedules Count: {schedules.Count()}");

            return View(schedules); // Pass schedules to the view
        }

        // Passenger List: Display passengers for a specific schedule
        public async Task<IActionResult> PassengerList(int scheduleId)
        {
            var passengers = await _driverService.GetPassengersByScheduleIdAsync(scheduleId);
            return View(passengers);
        }

        // Journey: Display journey details for a specific schedule


        public async Task<IActionResult> JourneyList()
        {
            var journeys = await _driverService.GetJourneyListAsync();

            if (!journeys.Any())
            {
                ViewBag.Message = "No journeys found.";
                return View(new List<object>());
            }

            return View(journeys);
        }
    }
}
