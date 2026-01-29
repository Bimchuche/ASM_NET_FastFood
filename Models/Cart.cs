using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    /// <summary>
    /// Model Cart - Navigation properties phải là virtual cho Lazy Loading
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }

        // ===== FK USER =====
        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }  // ✅ virtual cho Lazy Loading

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ virtual cho Lazy Loading
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
