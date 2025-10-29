using BookStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("OrderItems")]
public class ItemOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal PriceAtPurchase { get; set; } // In case book price changes later

    // Navigation
    public Order? Order { get; set; }
    public Book? Book { get; set; }
}