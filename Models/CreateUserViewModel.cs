using System.ComponentModel.DataAnnotations;

public class CreateUserViewModel
{
    [Required]
    public string FullName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required] // ✔ bắt buộc khi tạo
    public string Password { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public string Role { get; set; }
}
