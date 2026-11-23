using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class Supplier
{
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del proveedor admite hasta 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contacto del proveedor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El contacto del proveedor admite hasta 100 caracteres.")]
    public string Contact { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La nota del proveedor admite hasta 255 caracteres.")]
    public string? Note { get; set; }
}