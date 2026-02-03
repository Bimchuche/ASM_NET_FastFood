using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models;

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
    public string PaymentMethod { get; set; } = "COD";

    [Required]
    public int CustomerId { get; set; }
    public virtual User? Customer { get; set; }

    public int? ShipperId { get; set; }
    public virtual User? Shipper { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        = new List<OrderDetail>();

    [Display(Name = "Ảnh xác nhận giao hàng")]
    public string? DeliveryProofImageUrl { get; set; }
    
    [Display(Name = "Ngày giao")]
    [DataType(DataType.DateTime)]
    public DateTime? DeliveryDate { get; set; }

    [Display(Name = "Thời điểm xác nhận")]
    public DateTime? ConfirmedAt { get; set; }
    
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public double? ShipperLatitude { get; set; }
    public double? ShipperLongitude { get; set; }
    
    [Display(Name = "Thời gian dự kiến (phút)")]
    public int? EstimatedMinutes { get; set; }

    [Display(Name = "Trạng thái thanh toán")]
    public string? PaymentStatus { get; set; }
    
    [Display(Name = "PayOS Link ID")]
    public string? PaymentLinkId { get; set; }
    
    [Display(Name = "PayOS Order Code")]
    public long? PaymentOrderCode { get; set; }
    
    [Display(Name = "Mã giảm giá")]
    public int? CouponId { get; set; }
    public virtual Coupon? Coupon { get; set; }
    
    [Display(Name = "Số tiền giảm")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Display(Name = "Thời điểm hủy")]
    public DateTime? CancelledAt { get; set; }

    [Display(Name = "Đã xóa")]
    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Ngày xóa")]
    public DateTime? DeletedAt { get; set; }
}
