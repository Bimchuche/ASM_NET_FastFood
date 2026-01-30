using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.ViewModels
{
    public class CreateComboViewModel
    {
        [Required(ErrorMessage = "Tên combo là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên từ 2-200 ký tự")]
        [Display(Name = "Tên combo")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Giá combo là bắt buộc")]
        [Range(1000, 50000000, ErrorMessage = "Giá từ 1,000đ - 50,000,000đ")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ảnh combo")]
        public IFormFile? ImageFile { get; set; }

        public List<FoodCheckboxItem> FoodList { get; set; } = new();
        
        public List<FoodCheckboxItem> DrinkList { get; set; } = new();
    }

    public class FoodCheckboxItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = "";
        public bool IsSelected { get; set; }
        
        [Range(1, 100, ErrorMessage = "Số lượng từ 1-100")]
        public int Quantity { get; set; } = 1;
    }
}
