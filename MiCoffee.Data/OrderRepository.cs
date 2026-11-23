using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class OrderRepository
{
    // Column order matters: MapOrder reads by position.
    private const string SelectOrder =
        "SELECT co.customer_order_id, co.created_at, co.customer_name, co.total, co.status " +
        "FROM customer_order co ";

    // Column order matters: AttachItems reads by position.
    private const string SelectItems =
        "SELECT oi.order_item_id, oi.customer_order_id, oi.product_id, p.name, " +
        "oi.quantity, oi.unit_price, oi.note " +
        "FROM order_item oi " +
        "JOIN product p ON p.product_id = oi.product_id " +
        "JOIN customer_order co ON co.customer_order_id = oi.customer_order_id ";

    // Column order matters: AttachItems reads by position.
    private const string SelectItemOptions =
        "SELECT oio.order_item_option_id, oio.order_item_id, oio.product_option_id, " +
        "po.name, oio.extra_price " +
        "FROM order_item_option oio " +
        "JOIN product_option po ON po.product_option_id = oio.product_option_id " +
        "JOIN order_item oi ON oi.order_item_id = oio.order_item_id " +
        "JOIN customer_order co ON co.customer_order_id = oi.customer_order_id ";

    // Filters over the "co" alias, shared by the order query and the item and
    // option queries so all three read the same orders.
    private const string ActiveFilter = "co.status IN ('Received', 'InPreparation', 'Ready') ";
    private const string ByIdFilter = "co.customer_order_id = @customer_order_id ";

    /// <summary>
    /// Inserts the order, its items and their options in one transaction and
    /// returns the new CustomerOrderId. Returns null, without changes, when the
    /// database rejects an item because its product is not available.
    /// </summary>
    public int? Insert(CustomerOrder order)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        try
        {
            int orderId;
            using (MySqlCommand cmd = new(
                "INSERT INTO customer_order (customer_name, total, status) " +
                "VALUES (@customer_name, @total, @status);", cn, tx))
            {
                cmd.Parameters.AddWithValue("@customer_name", (object?)order.CustomerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@total", order.Total);
                cmd.Parameters.AddWithValue("@status", order.Status.ToString());
                cmd.ExecuteNonQuery();
                orderId = (int)cmd.LastInsertedId;
            }

            foreach (OrderItem item in order.Items)
            {
                int itemId;
                using (MySqlCommand cmd = new(
                    "INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price, note) " +
                    "VALUES (@customer_order_id, @product_id, @quantity, @unit_price, @note);", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@customer_order_id", orderId);
                    cmd.Parameters.AddWithValue("@product_id", item.ProductId);
                    cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                    cmd.Parameters.AddWithValue("@unit_price", item.UnitPrice);
                    cmd.Parameters.AddWithValue("@note", (object?)item.Note ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    itemId = (int)cmd.LastInsertedId;
                }

                foreach (OrderItemOption option in item.Options)
                {
                    using MySqlCommand cmd = new(
                        "INSERT INTO order_item_option (order_item_id, product_option_id, extra_price) " +
                        "VALUES (@order_item_id, @product_option_id, @extra_price);", cn, tx);
                    cmd.Parameters.AddWithValue("@order_item_id", itemId);
                    cmd.Parameters.AddWithValue("@product_option_id", option.ProductOptionId);
                    cmd.Parameters.AddWithValue("@extra_price", option.ExtraPrice);
                    cmd.ExecuteNonQuery();
                }
            }

            tx.Commit();
            return orderId;
        }
        catch (MySqlException ex) when (MySqlErrors.IsRejectedByTrigger(ex))
        {
            tx.Rollback();
            return null;
        }
    }

    /// <summary>
    /// Orders in Received, InPreparation or Ready, from the oldest to the
    /// newest, with Items, Options and the product and option names loaded.
    /// StatusHistory is not loaded.
    /// </summary>
    public List<CustomerOrder> GetActive()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        List<CustomerOrder> orders;
        using (MySqlCommand cmd = new(
            SelectOrder + "WHERE " + ActiveFilter +
            "ORDER BY co.created_at, co.customer_order_id;", cn))
        {
            orders = ReadOrders(cmd);
        }

        AttachItems(cn, orders, ActiveFilter, null);
        return orders;
    }

    /// <summary>
    /// One order with Items, Options (with names) and StatusHistory (with the
    /// name of the user), or null.
    /// </summary>
    public CustomerOrder? GetById(int orderId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        CustomerOrder? order;
        using (MySqlCommand cmd = new(SelectOrder + "WHERE " + ByIdFilter + ";", cn))
        {
            cmd.Parameters.AddWithValue("@customer_order_id", orderId);
            order = ReadOrders(cmd).FirstOrDefault();
        }

        if (order is null)
        {
            return null;
        }

        AttachItems(cn, new List<CustomerOrder> { order }, ByIdFilter,
            command => command.Parameters.AddWithValue("@customer_order_id", orderId));
        AttachHistory(cn, order);

        return order;
    }

    /// <summary>
    /// Current status of the order, or null when it does not exist.
    /// </summary>
    public OrderStatus? GetStatus(int orderId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT status FROM customer_order WHERE customer_order_id = @customer_order_id;", cn);
        cmd.Parameters.AddWithValue("@customer_order_id", orderId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Enum.Parse<OrderStatus>(reader.GetString(0)) : null;
    }

    /// <summary>
    /// Changes the status and records who changed it by calling
    /// sp_change_order_status, which runs both writes in one transaction.
    /// The change only applies while the order still has previousStatus, so
    /// two simultaneous changes cannot overwrite each other. Returns false,
    /// without changes, when the order no longer has previousStatus or when
    /// the database rejects the change (a transition that is not allowed, an
    /// order that does not exist or a status that is not valid).
    /// </summary>
    public bool ChangeStatus(int orderId, OrderStatus previousStatus, OrderStatus newStatus, int userId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        try
        {
            using MySqlCommand cmd = new(
                "CALL sp_change_order_status(@customer_order_id, @previous_status, @new_status, @user_id);", cn);
            cmd.Parameters.AddWithValue("@customer_order_id", orderId);
            cmd.Parameters.AddWithValue("@previous_status", previousStatus.ToString());
            cmd.Parameters.AddWithValue("@new_status", newStatus.ToString());
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (MySqlException ex) when (MySqlErrors.IsRejectedByTrigger(ex))
        {
            return false;
        }
    }

    /// <summary>
    /// Orders without items, from the newest to the oldest. Every filter is
    /// optional. fromDate and toDate compare against created_at and include
    /// the whole day of both ends; status is the current status of the order.
    /// </summary>
    public List<CustomerOrder> Search(DateOnly? fromDate, DateOnly? toDate, OrderStatus? status)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new() { Connection = cn };
        List<string> conditions = new();

        if (fromDate is not null)
        {
            conditions.Add("co.created_at >= @from_date");
            cmd.Parameters.AddWithValue("@from_date", fromDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (toDate is not null)
        {
            conditions.Add("co.created_at < DATE_ADD(@to_date, INTERVAL 1 DAY)");
            cmd.Parameters.AddWithValue("@to_date", toDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (status is not null)
        {
            conditions.Add("co.status = @status");
            cmd.Parameters.AddWithValue("@status", status.Value.ToString());
        }

        string where = conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions) + " ";
        cmd.CommandText = SelectOrder + where + "ORDER BY co.created_at DESC, co.customer_order_id DESC;";

        return ReadOrders(cmd);
    }

    // The reader is closed when this method returns, so the caller can run
    // another command on the same connection.
    private static List<CustomerOrder> ReadOrders(MySqlCommand cmd)
    {
        using MySqlDataReader reader = cmd.ExecuteReader();
        List<CustomerOrder> orders = new();
        while (reader.Read())
        {
            orders.Add(MapOrder(reader));
        }

        return orders;
    }

    // Loads the items and options of the orders selected by the filter. An
    // order that changed between the queries is not in the list, so its rows
    // are skipped.
    private static void AttachItems(
        MySqlConnection cn, List<CustomerOrder> orders, string filter, Action<MySqlCommand>? bind)
    {
        Dictionary<int, CustomerOrder> ordersById = orders.ToDictionary(o => o.CustomerOrderId);
        Dictionary<int, OrderItem> itemsById = new();

        using (MySqlCommand cmd = new(
            SelectItems + "WHERE " + filter + "ORDER BY oi.order_item_id;", cn))
        {
            bind?.Invoke(cmd);

            using MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!ordersById.TryGetValue(reader.GetInt32(1), out CustomerOrder? order))
                {
                    continue;
                }

                OrderItem item = new()
                {
                    OrderItemId = reader.GetInt32(0),
                    CustomerOrderId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    Quantity = reader.GetInt32(4),
                    UnitPrice = reader.GetDecimal(5),
                    Note = reader.IsDBNull(6) ? null : reader.GetString(6)
                };

                order.Items.Add(item);
                itemsById[item.OrderItemId] = item;
            }
        }

        using (MySqlCommand cmd = new(
            SelectItemOptions + "WHERE " + filter + "ORDER BY oio.order_item_option_id;", cn))
        {
            bind?.Invoke(cmd);

            using MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!itemsById.TryGetValue(reader.GetInt32(1), out OrderItem? item))
                {
                    continue;
                }

                item.Options.Add(new OrderItemOption
                {
                    OrderItemOptionId = reader.GetInt32(0),
                    OrderItemId = item.OrderItemId,
                    ProductOptionId = reader.GetInt32(2),
                    OptionName = reader.GetString(3),
                    ExtraPrice = reader.GetDecimal(4)
                });
            }
        }
    }

    private static void AttachHistory(MySqlConnection cn, CustomerOrder order)
    {
        using MySqlCommand cmd = new(
            "SELECT h.order_status_history_id, h.user_id, h.previous_status, h.new_status, " +
            "h.changed_at, u.full_name " +
            "FROM order_status_history h " +
            "JOIN `user` u ON u.user_id = h.user_id " +
            "WHERE h.customer_order_id = @customer_order_id " +
            "ORDER BY h.changed_at, h.order_status_history_id;", cn);
        cmd.Parameters.AddWithValue("@customer_order_id", order.CustomerOrderId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderStatusHistoryId = reader.GetInt32(0),
                CustomerOrderId = order.CustomerOrderId,
                UserId = reader.GetInt32(1),
                PreviousStatus = Enum.Parse<OrderStatus>(reader.GetString(2)),
                NewStatus = Enum.Parse<OrderStatus>(reader.GetString(3)),
                ChangedAt = reader.GetDateTime(4),
                ChangedByName = reader.GetString(5)
            });
        }
    }

    private static CustomerOrder MapOrder(MySqlDataReader reader)
    {
        return new CustomerOrder
        {
            CustomerOrderId = reader.GetInt32(0),
            CreatedAt = reader.GetDateTime(1),
            CustomerName = reader.IsDBNull(2) ? null : reader.GetString(2),
            Total = reader.GetDecimal(3),
            Status = Enum.Parse<OrderStatus>(reader.GetString(4))
        };
    }
}