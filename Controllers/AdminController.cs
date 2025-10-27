using BookStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }


        // Menampilkan daftar user
        public async Task<IActionResult> UsersList()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }


        //Menampilkan daftar pesanan (dilihat dari statusnya)
        public async Task<IActionResult> OrdersList()
        {
            var messages = await _context.ContactMessages.ToListAsync();
            return View(messages);
        }




    }
}