using BookStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace BookStore.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending"; // pilihan: Pending, Confirmed, Shipped, Delivered

        // Navigation
        public ApplicationUser? User { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ICollection<ItemOrder> OrderItems { get; set; } = new List<ItemOrder>();
    }
}

