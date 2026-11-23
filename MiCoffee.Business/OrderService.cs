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