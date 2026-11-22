using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class Product
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    public int ProductCategoryId { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del producto admite hasta 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999.99, ErrorMessage = "El precio base debe estar entre 0 y 99,999,999.99.")]
    public decimal BasePrice { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Loaded by ProductService reads (GetAll, GetById, GetMenu). Never bound
    // from a form: the allowed options are changed only through
    // ProductService.SetAllowedOptions.
    public ProductCategory? Category { get; set; }

    public List<ProductOption> AllowedOptions { get; set; } = new();
}
