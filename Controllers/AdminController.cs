using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;
using YatriSewa.Services.Interfaces;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IOperatorService _operatorService;

        // Modify the constructor to accept ILogger<OperatorController>
        public AdminController(ApplicationContext context, IOperatorService operatorService, ILogger<AdminController> logger)
        {
            _context = context; // Assigning the database context
            _operatorService = operatorService; // Assigning the operator service
            _logger = logger; // Assigning the logger
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

        // GET: Admin/Operators
        public async Task<IActionResult> ListOperators()
        {
            var operators = await _operatorService.GetAllOperatorsAsync();
            return View(operators); // Pass the list of operators to the view
        }


    }
}
