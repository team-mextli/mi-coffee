using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

/// <summary>
/// One line of an order: a product, its quantity and the options chosen.
/// </summary>
public class OrderItem
{
    public int OrderItemId { get; set; }

    public int CustomerOrderId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El producto es obligatorio.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Quantity { get; set; }

    // Read from the catalog by OrderService, never taken from a form or a
    // cart. Copied to the order so later price changes do not affect it.
    public decimal UnitPrice { get; set; }

    // Name of the product in the catalog, loaded only when an order is read
    // for the staff. Never taken from a form or a cart: OrderService.Create
    // ignores it.
    public string? ProductName { get; set; }

    [StringLength(255, ErrorMessage = "La nota admite hasta 255 caracteres.")]
    public string? Note { get; set; }

    public List<OrderItemOption> Options { get; set; } = new();
}
