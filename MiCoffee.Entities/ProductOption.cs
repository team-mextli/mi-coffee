using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

/// <summary>
/// Customization option of a product (size, milk, sweetener, extra).
/// </summary>
public class ProductOption
{
    public int ProductOptionId { get; set; }

    [Required(ErrorMessage = "El grupo de la opción es obligatorio.")]
    [StringLength(50, ErrorMessage = "El grupo de la opción admite hasta 50 caracteres.")]
    public string OptionGroup { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la opción es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de la opción admite hasta 50 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999.99, ErrorMessage = "El precio adicional debe estar entre 0 y 99,999,999.99.")]
    public decimal ExtraPrice { get; set; }
}
