using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    /// <summary>
    /// Model Category với Data Annotations đầy đủ
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên từ 2-100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        // Navigation Property - virtual cho Lazy Loading
        public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

        // ===== SOFT DELETE =====
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }
    }
}
