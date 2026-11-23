using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

public class Supply
{
    public int SupplyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    public int SupplyCategoryId { get; set; }

    // Null when the supply has no supplier assigned.
    public int? SupplierId { get; set; }

    [Required(ErrorMessage = "El nombre del insumo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del insumo admite hasta 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(20, ErrorMessage = "La unidad de medida admite hasta 20 caracteres.")]
    public string Unit { get; set; } = string.Empty;

    // Initial stock on Create. SupplyService.Update ignores it: after the
    // supply exists, the stock changes only through AddStock and SetStock.
    [Range(0, 99999999.99, ErrorMessage = "La existencia debe estar entre 0 y 99,999,999.99.")]
    public decimal CurrentStock { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El límite mínimo debe estar entre 0 y 99,999,999.99.")]
    public decimal MinimumStock { get; set; }

    // Loaded by GetAll, GetById and GetAlerts (Supplier is null when none is
    // assigned). Never bound from a form: the services use the ids above.
    public SupplyCategory? Category { get; set; }

    public Supplier? Supplier { get; set; }
}