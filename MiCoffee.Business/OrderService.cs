using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Customer orders.
/// </summary>
public class OrderService
{
    /// <summary>
    /// Validates the order, copies the current catalog prices to it,
    /// calculates its total and saves it with its items and options in a
    /// single transaction. Returns the order number (CustomerOrderId).
    /// The order is saved as Received. Any Total, Status, CreatedAt,
    /// UnitPrice or ExtraPrice in the argument is ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// No items, Quantity below 1, the product does not exist or is not
    /// available, an option does not exist, is not allowed for the product or
    /// is repeated within an item, or CustomerName or Note too long.
    /// </exception>
    public int Create(CustomerOrder order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Total of one line: (product base price + sum of the option extra
    /// prices) × Quantity, with the prices read from the catalog. The prices
    /// in the argument are ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Quantity below 1, the product does not exist or is not available, or
    /// an option does not exist, is not allowed for the product or is
    /// repeated within the item.
    /// </exception>
    public decimal CalculateLineTotal(OrderItem item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sum of the line totals, calculated as in CalculateLineTotal. Returns 0
    /// for an order without items.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Any item fails the rules of CalculateLineTotal.
    /// </exception>
    public decimal CalculateTotal(CustomerOrder order)
    {
        throw new NotImplementedException();
    }
}
