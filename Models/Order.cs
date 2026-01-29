using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models;

/// <summary>
/// Model Order với Data Annotations đầy đủ
/// </summary>
public class Order
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Mã đơn hàng là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Mã đơn hàng")]
    public string OrderCode { get; set; } = "";

    [Display(Name = "Ngày đặt")]
    [DataType(DataType.DateTime)]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "Pending";

    [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải >= 0")]
    [Display(Name = "Tổng tiền")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = "";

    [Display(Name = "Phương thức thanh toán")]
    public string PaymentMethod { get; set; } = "COD"; // COD, QR

    // FK - Customer
    [Required]
    public int CustomerId { get; set; }
    public virtual User? Customer { get; set; }

    // FK - Shipper (optional)
    public int? ShipperId { get; set; }
    public virtual User? Shipper { get; set; }

    // Navigation Property
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        = new List<OrderDetail>();

    // Proof of delivery
    [Display(Name = "Ảnh xác nhận giao hàng")]
    public string? DeliveryProofImageUrl { get; set; }
    
    [Display(Name = "Ngày giao")]
    [DataType(DataType.DateTime)]
    public DateTime? DeliveryDate { get; set; }

    // Estimated delivery time
    [Display(Name = "Thời điểm xác nhận")]
    public DateTime? ConfirmedAt { get; set; }
    
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public double? ShipperLatitude { get; set; }
    public double? ShipperLongitude { get; set; }
    
    [Display(Name = "Thời gian dự kiến (phút)")]
    public int? EstimatedMinutes { get; set; }

    // ===== SOFT DELETE =====
    [Display(Name = "Đã xóa")]
    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Ngày xóa")]
    public DateTime? DeletedAt { get; set; }
}
