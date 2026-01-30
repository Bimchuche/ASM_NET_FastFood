namespace ASM1_NET.Models;

public class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public int? FoodId { get; set; }
    public virtual Food? Food { get; set; }

    public int? ComboId { get; set; }
    public virtual Combo? Combo { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
