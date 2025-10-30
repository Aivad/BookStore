using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookStore.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public IActionResult Index()
        {
            var model = new Contact();

            // Auto-fill if user is logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    model.Name = user.UserName;  // or FirstName if you have it
                    model.Email = user.Email;
                }
            }

            return View(model);
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Contact model)
        {
            if (ModelState.IsValid)
            {
                model.SentAt = DateTime.UtcNow;
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Terimakasih atas pesan yang diberikan, Tim kami akan periksa dengan segera.";
                return RedirectToAction("Index");
            }

            // Bagian untuk user yang telah login 
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(model.Name)) model.Name = user.UserName;
                    if (string.IsNullOrWhiteSpace(model.Email)) model.Email = user.Email;
                }
            }

            return View(model);
        }
    }
}