using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class User
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre completo admite hasta 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario admite hasta 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    // Set only by Data and Business. Never bound from a form: the plain
    // password travels as a separate parameter of UserService.
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;
}