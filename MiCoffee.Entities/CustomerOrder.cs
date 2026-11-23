using System.ComponentModel.DataAnnotations;

namespace MiCoffee.Entities;

/// <summary>
/// Confirmed order. CustomerOrderId is also the order number shown to the
/// customer.
/// </summary>
public class CustomerOrder
{
    public int CustomerOrderId { get; set; }

    // Set when the order is saved. Ignored by OrderService.Create.
    public DateTime CreatedAt { get; set; }

    [StringLength(50, ErrorMessage = "El nombre del cliente admite hasta 50 caracteres.")]
    public string? CustomerName { get; set; }

    // Calculated by OrderService. Never taken from a form or a cart.
    public decimal Total { get; set; }

    // A new order is always saved as Received.
    public OrderStatus Status { get; set; } = OrderStatus.Received;

    public List<OrderItem> Items { get; set; } = new();

    // Loaded only when a single order is read for its receipt; empty
    // otherwise. Never taken from a form: OrderService.Create ignores it.
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
}
