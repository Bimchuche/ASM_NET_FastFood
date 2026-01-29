using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models
{
    /// <summary>
    /// Model Combo với Data Annotations đầy đủ
    /// </summary>
    public class Combo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên combo là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên từ 2-200 ký tự")]
        [Display(Name = "Tên combo")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Giá combo là bắt buộc")]
        [Range(1000, 50000000, ErrorMessage = "Giá từ 1,000đ - 50,000,000đ")]
        [Display(Name = "Giá bán")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        [Display(Name = "Tải ảnh mới")]
        public IFormFile? ImageFile { get; set; }

        // Navigation Property
        public virtual ICollection<ComboDetail>? ComboDetails { get; set; }

        // ===== SOFT DELETE =====
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }
    }
}
