using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    [Table("CartItems")]
    public class Cart
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
        public string UserId { get; set; } = string.Empty;

        //Navigation property
        public ApplicationUser? User { get; set; }
        public Book? Book { get; set; }
    }
}
