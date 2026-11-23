using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class SupplyCategory
{
    public int SupplyCategoryId { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría de insumo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de la categoría de insumo admite hasta 50 caracteres.")]
    public string Name { get; set; } = string.Empty;
}