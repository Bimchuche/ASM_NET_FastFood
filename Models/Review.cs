using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? FoodId { get; set; }

        public int? ComboId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("FoodId")]
        public virtual Food? Food { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }
    }
}
