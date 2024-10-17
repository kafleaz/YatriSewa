using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YatriSewa.Controllers
{
    [Authorize(Roles = "Operator")]
    public class OperatorController : Controller
    {
        public IActionResult OperatorDashboard()
        {
            return View();
        }
    }
}
