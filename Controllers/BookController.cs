using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            return View(books);
        }

        // Pencarian buku terkait
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var books = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(query) || b.Author.Contains(query))
                .ToListAsync();
            return View("SearchResults", books);
        }

        // Detail buku
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }
    }
}