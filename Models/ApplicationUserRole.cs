using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class ApplicationUserRole
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        // Navigation properties (optional)
        // public ApplicationUser User { get; set; }
        // public ApplicationRole Role { get; set; }
    }
}