using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart? Cart { get; set; }

        public int? FoodId { get; set; }
        public virtual Food? Food { get; set; }

        public int? ComboId { get; set; }
        public virtual Combo? Combo { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
