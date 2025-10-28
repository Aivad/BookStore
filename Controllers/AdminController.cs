using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult IndexUser()
        {
            return View("IndexUser");
        }

        public IActionResult IndexBook()
        {
            return View("IndexBook");
        }
    }
}