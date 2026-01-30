using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models;

public class Food
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên từ 2-200 ký tự")]
    [Display(Name = "Tên món")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(1000, 10000000, ErrorMessage = "Giá từ 1,000đ - 10,000,000đ")]
    [Display(Name = "Giá bán")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Hình ảnh")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Còn bán")]
    public bool IsAvailable { get; set; } = true;

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }
    
    public virtual Category? Category { get; set; }

    [Display(Name = "Đã xóa")]
    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Ngày xóa")]
    public DateTime? DeletedAt { get; set; }
}
