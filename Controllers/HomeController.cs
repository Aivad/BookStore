using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Halaman awal aplikasi.
        /// - Jika user belum login → arahkan ke halaman Login (Identity)
        /// - Jika sudah login → arahkan ke daftar buku
        /// </summary>
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // User sudah login → tampilkan daftar buku
                return RedirectToAction("Index", "Book");
            }
            else
            {
                // Belum login → arahkan ke halaman Login Identity
                return RedirectToAction("Login", "Account");
            }
        }

        /// <summary>
        /// Halaman "Tentang Kami" — bisa diakses semua orang (tanpa login)
        /// </summary>
        public IActionResult About() => View();
    }
}