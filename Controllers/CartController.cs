using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }


        //Menambahkan buku ke cart
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var userId = User.Identity?.Name; // ID menggunakan email
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.BookId == bookId && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                _context.CartItems.Add(new Cart
                {
                    BookId = bookId,
                    UserId = userId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Book");
        }


        // Menampilkan isi cart
        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.Name;
            var cart = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Book)
                .ToListAsync();
            return View(cart);
        }
    }
}