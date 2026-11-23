namespace MiCoffee.Entities;

/// <summary>
/// One status change of an order: who made it and when. Written only by the
/// status change of OrderService.
/// </summary>
public class OrderStatusHistory
{
    public int OrderStatusHistoryId { get; set; }

    public int CustomerOrderId { get; set; }

    public int UserId { get; set; }

    public OrderStatus PreviousStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    // Full name of the user, loaded together with the row.
    public string ChangedByName { get; set; } = string.Empty;
}
