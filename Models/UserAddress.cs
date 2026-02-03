using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models;

public class UserAddress
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên địa chỉ")]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(500)]
    [Display(Name = "Địa chỉ đầy đủ")]
    public string FullAddress { get; set; } = "";

    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Display(Name = "Mặc định")]
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
