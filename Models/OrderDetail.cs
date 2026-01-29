namespace ASM1_NET.Models;

/// <summary>
/// Model OrderDetail - virtual cho Lazy Loading
/// </summary>
public class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }  // ✅ virtual

    public int? FoodId { get; set; }
    public virtual Food? Food { get; set; }  // ✅ virtual

    public int? ComboId { get; set; }
    public virtual Combo? Combo { get; set; }  // ✅ virtual

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
