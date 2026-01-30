namespace ASM1_NET.Models
{
    public class ComboDetail
    {
        public int ComboId { get; set; }
        public virtual Combo? Combo { get; set; }

        public int FoodId { get; set; }
        public virtual Food? Food { get; set; }

        public int Quantity { get; set; }
    }
}
