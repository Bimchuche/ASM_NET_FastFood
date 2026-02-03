using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models;

public class Wishlist
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Required]
    public int FoodId { get; set; }

    [ForeignKey("FoodId")]
    public virtual Food? Food { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
