using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

/// <summary>
/// An option chosen for one order line.
/// </summary>
public class OrderItemOption
{
    public int OrderItemOptionId { get; set; }

    public int OrderItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La opción es obligatoria.")]
    public int ProductOptionId { get; set; }

    // Read from the catalog by OrderService, never taken from a form or a
    // cart. Copied to the order so later price changes do not affect it.
    public decimal ExtraPrice { get; set; }

    // Name of the option in the catalog, loaded only when an order is read
    // for the staff. Never taken from a form or a cart: OrderService.Create
    // ignores it.
    public string? OptionName { get; set; }
}
