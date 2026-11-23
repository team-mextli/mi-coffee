using System.Text.Json;
using MiCoffee.Entities;

namespace MiCoffee.Web.Cart;

/// <summary>
/// The customer cart, kept in the browser session as a list of OrderItem.
/// Only ProductId, Quantity, Note and the chosen ProductOptionId values are
/// stored: prices are always read from the catalog by OrderService.
/// </summary>
public static class CartSession
{
    private const string Key = "Cart";

    public static List<OrderItem> Get(ISession session)
    {
        string? json = session.GetString(Key);
        if (string.IsNullOrEmpty(json))
        {
            return new List<OrderItem>();
        }

        return JsonSerializer.Deserialize<List<OrderItem>>(json) ?? new List<OrderItem>();
    }

    public static void Save(ISession session, List<OrderItem> items)
    {
        session.SetString(Key, JsonSerializer.Serialize(items));
    }

    public static void Clear(ISession session)
    {
        session.Remove(Key);
    }

    public static int Count(ISession session)
    {
        return Get(session).Sum(item => item.Quantity);
    }
}
