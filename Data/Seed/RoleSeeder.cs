using BookStore.Data;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BookStore.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Only seed if no roles exist
            if (await context.Roles.AnyAsync()) return;

            var roles = new[] { "Admin", "User" };

            foreach (var roleName in roles)
            {
                context.Roles.Add(new ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName
                });
            }

            await context.SaveChangesAsync();
        }
    }
}