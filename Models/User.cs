using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [StringLength(255)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = "";

    [Required]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Customer";

    [Display(Name = "Hoạt động")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [Display(Name = "Đã xóa")]
    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Ngày xóa")]
    public DateTime? DeletedAt { get; set; }

    // Email verification
    [Display(Name = "Email đã xác thực")]
    public bool EmailVerified { get; set; } = false;

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationTokenExpiry { get; set; }

    // Password change verification
    public string? PasswordChangeOTP { get; set; }
    public DateTime? PasswordChangeOTPExpiry { get; set; }
    public string? NewPasswordPending { get; set; }

    // Loyalty Points
    [Display(Name = "Điểm tích lũy")]
    public int TotalPoints { get; set; } = 0;

    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = new List<LoyaltyPoint>();
}
