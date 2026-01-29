namespace ASM1_NET.Models
{
    /// <summary>
    /// Model ComboDetail - virtual cho Lazy Loading
    /// </summary>
    public class ComboDetail
    {
        public int ComboId { get; set; }
        public virtual Combo? Combo { get; set; }  // ✅ virtual

        public int FoodId { get; set; }
        public virtual Food? Food { get; set; }  // ✅ virtual

        public int Quantity { get; set; }
    }
}
