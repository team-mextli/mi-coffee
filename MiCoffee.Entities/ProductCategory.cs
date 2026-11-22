using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class ProductCategory
{
    public int ProductCategoryId { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de la categoría admite hasta 50 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
