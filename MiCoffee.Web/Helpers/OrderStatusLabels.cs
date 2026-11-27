using MiCoffee.Entities;

namespace MiCoffee.Web.Helpers;

/// <summary>
/// Spanish text and Bootstrap badge class for each order status.
/// Presentation only: the status rules live in OrderService.
/// </summary>
public static class OrderStatusLabels
{
    public static string Text(OrderStatus status) => status switch
    {
        OrderStatus.Received => "Recibida",
        OrderStatus.InPreparation => "En preparación",
        OrderStatus.Ready => "Lista",
        OrderStatus.Delivered => "Entregada",
        OrderStatus.Cancelled => "Cancelada",
        _ => status.ToString()
    };

    public static string BadgeClass(OrderStatus status) => status switch
    {
		OrderStatus.Received => "status-received",
		OrderStatus.InPreparation => "status-preparing",
		OrderStatus.Ready => "status-ready",
		OrderStatus.Delivered => "bg-success",
		OrderStatus.Cancelled => "bg-danger",
		_ => "text-bg-light"
	};

    // Text of the button that moves an order to the given status.
    public static string ActionText(OrderStatus status) => status switch
    {
        OrderStatus.InPreparation => "Iniciar preparación",
        OrderStatus.Ready => "Marcar como lista",
        OrderStatus.Delivered => "Marcar como entregada",
        OrderStatus.Cancelled => "Cancelar orden",
        _ => Text(status)
    };
}
