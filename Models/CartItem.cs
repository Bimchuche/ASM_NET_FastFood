using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models
{
    /// <summary>
    /// Model CartItem - Navigation properties phải là virtual cho Lazy Loading
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart? Cart { get; set; }  // ✅ virtual

        // ===== FOOD =====
        public int? FoodId { get; set; }
        public virtual Food? Food { get; set; }  // ✅ virtual

        // ===== COMBO =====
        public int? ComboId { get; set; }
        public virtual Combo? Combo { get; set; }  // ✅ virtual

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
