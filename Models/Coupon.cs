using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int UsageLimit { get; set; } = 0;

        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
