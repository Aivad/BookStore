using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

///<summary>
///Program Berikut merupakan program untuk menghandle tampilan home pada halaman setelah login
///</summary>

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        // Menampilkan semua buku
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.CartItemCount = cartItems.Sum(i => i.Quantity);

            return View(books);
        }

        // Pencarian buku terkait
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Jika tidak ada yang di query/cari di show all
                var Filteredbooks = await _context.Books
                    .Include(b => b.Category)
                    .ToListAsync();

                ViewBag.SearchQuery = "";
                return View("Index", Filteredbooks);
            }

            var books = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Title.ToLower().Contains(query.ToLower()) || b.Author.ToLower().Contains(query.ToLower()))
                .ToListAsync();

            ViewBag.SearchQuery = query; // Repopulate search box
            return View("Index", books);
        }

        // Detail buku
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }
    }
}