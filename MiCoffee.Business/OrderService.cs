using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Customer orders.
/// </summary>
public class OrderService
{
    private const decimal MaxTotal = 99999999.99m;

    private readonly OrderRepository _repository = new();
    private readonly ProductRepository _productRepository = new();

    // Allowed status changes: one step forward, or cancel while the order is
    // not final. Delivered and Cancelled have no next status.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> NextStatuses = new()
    {
        [OrderStatus.Received] = new[] { OrderStatus.InPreparation, OrderStatus.Cancelled },
        [OrderStatus.InPreparation] = new[] { OrderStatus.Ready, OrderStatus.Cancelled },
        [OrderStatus.Ready] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

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
    /// is repeated within an item, CustomerName or Note too long, or the total
    /// exceeds 99,999,999.99.
    /// </exception>
    public int Create(CustomerOrder order)
    {
        order.CustomerName = NormalizeOptional(order.CustomerName);
        if (order.CustomerName is { Length: > 50 })
        {
            throw new BusinessException("El nombre del cliente admite hasta 50 caracteres.");
        }

        if (order.Items.Count == 0)
        {
            throw new BusinessException("El pedido debe tener al menos un producto.");
        }

        Dictionary<int, Product?> products = new();
        decimal total = 0;
        foreach (OrderItem item in order.Items)
        {
            item.Note = NormalizeOptional(item.Note);
            if (item.Note is { Length: > 255 })
            {
                throw new BusinessException("La nota admite hasta 255 caracteres.");
            }

            total += PriceItem(item, products);
        }

        if (total > MaxTotal)
        {
            throw new BusinessException("El total del pedido excede el máximo permitido.");
        }

        order.Total = total;
        order.Status = OrderStatus.Received;

        // Null means the database rejected an item whose product stopped
        // being available after it was read above.
        int orderId = _repository.Insert(order)
            ?? throw new BusinessException("Alguno de los productos ya no está disponible.");

        order.CustomerOrderId = orderId;
        return orderId;
    }

    /// <summary>
    /// Total of one line: (product base price + sum of the option extra
    /// prices) × Quantity, with the prices read from the catalog. The prices
    /// in the argument are ignored and replaced by the catalog prices.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Quantity below 1, the product does not exist or is not available, or
    /// an option does not exist, is not allowed for the product or is
    /// repeated within the item.
    /// </exception>
    public decimal CalculateLineTotal(OrderItem item)
    {
        return PriceItem(item, new Dictionary<int, Product?>());
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
        Dictionary<int, Product?> products = new();
        return order.Items.Sum(item => PriceItem(item, products));
    }

    /// <summary>
    /// Orders in Received, InPreparation or Ready, from the oldest to the
    /// newest, each with Items, their Options and the product and option
    /// names loaded. StatusHistory is not loaded.
    /// </summary>
    public List<CustomerOrder> GetActive()
    {
        return _repository.GetActive();
    }

    /// <summary>
    /// Returns one order as a receipt: Items, their Options with the product
    /// and option names, and StatusHistory from the oldest change to the
    /// newest. Returns null when the order does not exist.
    /// </summary>
    public CustomerOrder? GetById(int orderId)
    {
        return _repository.GetById(orderId);
    }

    /// <summary>
    /// Statuses an order can move to from the given status, so Web can offer
    /// only valid changes without repeating the rule. Empty for Delivered and
    /// Cancelled.
    /// </summary>
    public IReadOnlyList<OrderStatus> GetNextStatuses(OrderStatus current)
    {
        return NextStatuses.TryGetValue(current, out OrderStatus[]? next)
            ? next
            : Array.Empty<OrderStatus>();
    }

    /// <summary>
    /// Changes the status of the order and records who changed it, in a
    /// single transaction. userId is the user who made the change.
    /// </summary>
    /// <exception cref="BusinessException">
    /// userId below 1, the order does not exist, the new status is not one of
    /// GetNextStatuses for its current status, or the order changed status
    /// while this change was being saved.
    /// </exception>
    public void ChangeStatus(int orderId, OrderStatus newStatus, int userId)
    {
        if (userId < 1)
        {
            throw new BusinessException("El usuario que realiza el cambio es obligatorio.");
        }

        OrderStatus current = _repository.GetStatus(orderId)
            ?? throw new BusinessException("La orden no existe.");

        if (!GetNextStatuses(current).Contains(newStatus))
        {
            throw new BusinessException(
                "La orden no puede pasar a ese estado desde su estado actual.");
        }

        // False means the order changed status after it was read above, or
        // the database rejected the change as a backup of the rule above.
        if (!_repository.ChangeStatus(orderId, current, newStatus, userId))
        {
            throw new BusinessException(
                "La orden cambió de estado mientras se actualizaba. Actualiza la lista e inténtalo de nuevo.");
        }
    }

    /// <summary>
    /// Order history without items, from the newest to the oldest. Every
    /// filter is optional: fromDate and toDate select the creation date (both
    /// days included) and status the current status of the order.
    /// </summary>
    /// <exception cref="BusinessException">
    /// fromDate is later than toDate.
    /// </exception>
    public List<CustomerOrder> Search(DateOnly? fromDate, DateOnly? toDate, OrderStatus? status)
    {
        if (fromDate is not null && toDate is not null && fromDate > toDate)
        {
            throw new BusinessException("La fecha inicial no puede ser posterior a la final.");
        }

        return _repository.Search(fromDate, toDate, status);
    }

    // Validates one line, copies the catalog prices into it and returns its
    // total. The products dictionary caches the catalog reads so a product
    // that appears in several lines is read once.
    private decimal PriceItem(OrderItem item, Dictionary<int, Product?> products)
    {
        if (item.Quantity < 1)
        {
            throw new BusinessException("La cantidad debe ser al menos 1.");
        }

        if (!products.TryGetValue(item.ProductId, out Product? product))
        {
            product = _productRepository.GetById(item.ProductId);
            products[item.ProductId] = product;
        }

        if (product is null)
        {
            throw new BusinessException("El producto no existe.");
        }

        if (!product.IsAvailable)
        {
            throw new BusinessException($"El producto «{product.Name}» no está disponible.");
        }

        if (item.Options.Select(o => o.ProductOptionId).Distinct().Count() != item.Options.Count)
        {
            throw new BusinessException("Una opción no puede repetirse en el mismo producto.");
        }

        item.UnitPrice = product.BasePrice;

        decimal extras = 0;
        foreach (OrderItemOption option in item.Options)
        {
            // An option that does not exist is never in the allowed list, so
            // this one check covers both cases.
            ProductOption? allowed = product.AllowedOptions
                .FirstOrDefault(o => o.ProductOptionId == option.ProductOptionId);

            if (allowed is null)
            {
                throw new BusinessException(
                    $"Una de las opciones no está permitida para «{product.Name}».");
            }

            option.ExtraPrice = allowed.ExtraPrice;
            extras += allowed.ExtraPrice;
        }

        return (item.UnitPrice + extras) * item.Quantity;
    }

    // Empty or whitespace-only text becomes null, as the columns are nullable.
    private static string? NormalizeOptional(string? value)
    {
        string trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}