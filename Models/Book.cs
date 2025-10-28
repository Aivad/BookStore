using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        [Required]
        [MaxLength(512)]
        public string? ImageUrl { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }
    }
}