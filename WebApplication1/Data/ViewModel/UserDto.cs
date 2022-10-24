using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data.ViewModel;

public class UserDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}