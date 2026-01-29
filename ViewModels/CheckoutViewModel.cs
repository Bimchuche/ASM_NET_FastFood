using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.ViewModels
{
    /// <summary>
    /// ViewModel cho form thanh toán - Data Validation
    /// </summary>
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3-9])\d{8}$", ErrorMessage = "SĐT Việt Nam không hợp lệ (10 số, bắt đầu 03-09)")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [Display(Name = "Ghi chú")]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }

        // Tọa độ giao hàng (tùy chọn)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
