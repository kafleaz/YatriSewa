using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<AdminController> _logger;

        // Modify the constructor to accept ILogger<OperatorController>
        public AdminController(ApplicationContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;  // Assign logger to the private field
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Operator, Admin")]
        public IActionResult ListBus()
        {
            // Redirect the request to OperatorController's ListBus action
            return RedirectToAction("ListBus", "Operator");
        }


    }
}
