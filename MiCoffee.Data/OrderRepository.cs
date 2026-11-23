using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class OrderRepository
{
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
}