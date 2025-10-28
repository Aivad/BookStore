using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BookStore.Data;
using BookStore.Models;
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

        public IActionResult IndexBook()
        {
            return View("IndexBook");
        }
    }
}