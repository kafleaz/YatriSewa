using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using YatriSewa.Models;

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
            return View();
        }
        public IActionResult Journey()
        {
            return View();
        }
        public IActionResult PassengerList(int busId)
        {
            // Assuming Role is an enum of type 'UserRole'
            var passengers = _context.User_Table
                .Where(u=> u.Role == UserRole.Passenger) // Use the enum value for comparison
                .Select(u => new User
                {
                   UserId = u.UserId,
                   Name= u.Name,
                   Email= u.Email,
                   Password= u.Password,
                   PhoneNo = u.PhoneNo /*u.PhoneNumber*/
                })
                .ToList();

            return View(passengers);
        }


    }
}
