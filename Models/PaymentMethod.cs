using BookStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace BookStore.Models
{
    [Table("PaymentMethods")]
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "Cash on Delivery";
    }
}

