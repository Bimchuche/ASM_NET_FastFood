using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models;

public class ShippingZone
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên khu vực")]
    public string Name { get; set; } = "";

    [Display(Name = "Khoảng cách tối thiểu (km)")]
    public double MinDistance { get; set; }

    [Display(Name = "Khoảng cách tối đa (km)")]
    public double MaxDistance { get; set; }

    [Display(Name = "Phí giao hàng")]
    public decimal ShippingFee { get; set; }

    [Display(Name = "Hoạt động")]
    public bool IsActive { get; set; } = true;
}
