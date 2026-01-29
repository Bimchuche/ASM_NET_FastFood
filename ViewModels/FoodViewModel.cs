using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.ViewModels
{
    /// <summary>
    /// ViewModel cho form tạo/sửa Food - Admin
    /// </summary>
    public class FoodViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên từ 2-200 ký tự")]
        [Display(Name = "Tên món")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(1000, 10000000, ErrorMessage = "Giá từ 1,000đ - 10,000,000đ")]
        [Display(Name = "Giá bán")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hình ảnh hiện tại")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Còn bán")]
        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
    }
}
