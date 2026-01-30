using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
