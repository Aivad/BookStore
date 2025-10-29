using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using System.Security.Claims;

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

        public IActionResult Dashboard() => View();

        // GET: /Admin/IndexUser
        public async Task<IActionResult> IndexUser(int page = 1)
        {
            const int pageSize = 10;
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalUsers = await _context.Users.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }

        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string Username, string Email, string Password, string Role)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("", "Username, email, and password are required.");
                return View();
            }

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == Email || u.UserName == Username))
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View();
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = Username,
                Email = Email,
                NormalizedUserName = Username.ToUpperInvariant(),
                NormalizedEmail = Email.ToUpperInvariant(),
                EmailConfirmed = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // Hash password
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign role
            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Role);
            if (roleEntity != null)
            {
                _context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = roleEntity.Id
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "User created successfully.";
            return RedirectToAction("IndexUser");
        }



        // POST: /Admin/DeleteUsers
        [HttpPost]
        public async Task<IActionResult> DeleteUsers(List<string> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var usersToDelete = await _context.Users
                    .Where(u => selectedIds.Contains(u.Id))
                    .ToListAsync();

                _context.Users.RemoveRange(usersToDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexUser");
        }

        // GET: /Admin/DetailUser/{id}
        public async Task<IActionResult> DetailUser(string id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();
            return View(user);
        }

        // GET: /Admin/EditUser/{id}
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /Admin/EditUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string NewPassword, string ConfirmPassword, string SelectedRole)
        {
            // Waiting connection to map the roles with users
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Re-populate model for validation errors
            var model = new ApplicationUser
            {
                Id = user.Id,
                UserName = Request.Form["UserName"],
                Email = Request.Form["Email"],
                ConcurrencyStamp = user.ConcurrencyStamp,
                SecurityStamp = user.SecurityStamp,
                UserRoles = user.UserRoles
            };

            // Validate password match
            if (!string.IsNullOrWhiteSpace(NewPassword) && NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View("EditUser", model);
            }

            // Update basic fields
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.NormalizedUserName = model.UserName.ToUpperInvariant();
            user.NormalizedEmail = model.Email.ToUpperInvariant();

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, NewPassword);
            }

            // Update role
            var currentRoleId = user.UserRoles.FirstOrDefault()?.RoleId;
            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == SelectedRole);

            if (newRole != null)
            {
                if (currentRoleId != newRole.Id)
                {
                    // Remove old role
                    if (currentRoleId != null)
                    {
                        var oldUserRole = await _context.UserRoles
                            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == currentRoleId);
                        if (oldUserRole != null)
                            _context.UserRoles.Remove(oldUserRole);
                    }

                    // Add new role
                    _context.UserRoles.Add(new ApplicationUserRole
                    {
                        UserId = user.Id,
                        RoleId = newRole.Id
                    });
                }
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("IndexUser");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving.");
                return View("EditUser", model);
            }
        }


        // GET: /Admin/IndexCategory
        public async Task<IActionResult> IndexCategory(int page = 1)
        {
            const int pageSize = 10;
            var categories = await _context.Categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCategories = await _context.Categories.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(categories);
        }

        // POST: /Admin/DeleteCategories
        [HttpPost]
        public async Task<IActionResult> DeleteCategories(List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var categoriesToDelete = await _context.Categories
                    .Where(c => selectedIds.Contains(c.Id))
                    .ToListAsync();

                _context.Categories.RemoveRange(categoriesToDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexCategory");
        }

        // GET: /Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction("IndexCategory");
        }

        // GET: /Admin/EditCategory/{id}
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Admin/EditCategory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model)
        {
            if (id != model.Id) return BadRequest();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            category.Name = model.Name;

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("IndexCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving.");
                return View(model);
            }
        }



        // GET: /Admin/IndexBook
        public async Task<IActionResult> IndexBook(int page = 1)
        {
            const int pageSize = 10;
            var books = await _context.Books
                .Include(b => b.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalBooks = await _context.Books.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(books);
        }

        // POST: /Admin/DeleteBooks
        [HttpPost]
        public async Task<IActionResult> DeleteBooks(List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var booksToDelete = await _context.Books
                    .Where(b => selectedIds.Contains(b.Id))
                    .ToListAsync();

                _context.Books.RemoveRange(booksToDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexBook");
        }

        // GET: /Admin/CreateBook
        public IActionResult CreateBook()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }

        // POST: /Admin/CreateBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(Book model, IFormFile? imageFile)
        {
            ViewData["Categories"] = _context.Categories.ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 25 * 1024 * 1024) // 25MB limit
                {
                    ModelState.AddModelError("imageFile", "Image size must be less than 25MB.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // Ensure folder exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    model.ImageUrl = "/uploads/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("imageFile", $"Failed to save image: {ex.Message}");
                    return View(model);
                }
            }
            else
            {
                // Use default image if no file uploaded
                model.ImageUrl = "/images/no_image_available.jpg";
            }

            _context.Books.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Book created successfully.";
            return RedirectToAction("IndexBook");
        }

        // GET: /Admin/EditBook/{id}
        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            return View(book);
        }

        // POST: /Admin/EditBook/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int id, Book model, IFormFile? imageFile)
        {
            ViewData["Categories"] = _context.Categories.ToList();

            if (id != model.Id) return BadRequest();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Update fields
            book.Title = model.Title;
            book.Author = model.Author;
            book.Description = model.Description;
            book.Price = model.Price;
            book.Stock = model.Stock;
            book.CategoryId = model.CategoryId;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 25 * 1024 * 1024) // 25MB limit
                {
                    ModelState.AddModelError("imageFile", "Image size must be less than 25MB.");
                    ViewData["Categories"] = _context.Categories.ToList();
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                book.ImageUrl = "/uploads/" + fileName;
            }

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Book updated successfully.";
                return RedirectToAction("IndexBook");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving.");
                ViewData["Categories"] = _context.Categories.ToList();
                return View(model);
            }
        }

        // GET: /Admin/IndexCart
        public async Task<IActionResult> IndexCart(int page = 1)
        {
            const int pageSize = 10;
            var cartItems = await _context.CartItems
                .Include(c => c.User)
                .Include(c => c.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _context.CartItems.CountAsync();
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            return View(cartItems);
        }

        // GET: /Admin/CreateCart
        public IActionResult CreateCart()
        {
            ViewData["Users"] = _context.Users.ToList();
            ViewData["Books"] = _context.Books.ToList();
            return View();
        }

        // POST: /Admin/CreateCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCart(Cart model)
        {
            if (ModelState.IsValid)
            {
                _context.CartItems.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("IndexCart");
            }
            ViewData["Users"] = _context.Users.ToList();
            ViewData["Books"] = _context.Books.ToList();
            return View(model);
        }

        // GET: /Admin/EditCart/5
        public async Task<IActionResult> EditCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Users"] = _context.Users.ToList();
            ViewData["Books"] = _context.Books.ToList();
            return View(item);
        }

        // POST: /Admin/EditCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCart(int id, Cart model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("IndexCart");
            }
            ViewData["Users"] = _context.Users.ToList();
            ViewData["Books"] = _context.Books.ToList();
            return View(model);
        }

        // POST: /Admin/DeleteCart
        [HttpPost]
        public async Task<IActionResult> DeleteCart(List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var items = await _context.CartItems
                    .Where(c => selectedIds.Contains(c.Id))
                    .ToListAsync();
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("IndexCart");
        }


    }
}