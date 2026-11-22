using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class ProductOptionRepository
{
    // Column order matters: Map reads by position.
    private const string SelectColumns =
        "product_option_id, option_group, name, extra_price";

    public List<ProductOption> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM product_option ORDER BY option_group, name;", cn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        List<ProductOption> options = new();
        while (reader.Read())
        {
            options.Add(Map(reader));
        }

        return options;
    }

    public ProductOption? GetById(int productOptionId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM product_option " +
            "WHERE product_option_id = @product_option_id;", cn);
        cmd.Parameters.AddWithValue("@product_option_id", productOptionId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(ProductOption option)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO product_option (option_group, name, extra_price) " +
            "VALUES (@option_group, @name, @extra_price);", cn);
        cmd.Parameters.AddWithValue("@option_group", option.OptionGroup);
        cmd.Parameters.AddWithValue("@name", option.Name);
        cmd.Parameters.AddWithValue("@extra_price", option.ExtraPrice);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    public void Update(ProductOption option)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE product_option SET option_group = @option_group, name = @name, " +
            "extra_price = @extra_price WHERE product_option_id = @product_option_id;", cn);
        cmd.Parameters.AddWithValue("@option_group", option.OptionGroup);
        cmd.Parameters.AddWithValue("@name", option.Name);
        cmd.Parameters.AddWithValue("@extra_price", option.ExtraPrice);
        cmd.Parameters.AddWithValue("@product_option_id", option.ProductOptionId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the option and its assignments to products, in one transaction.
    /// Returns false, without changes, when an order still references the
    /// option (the foreign key is the guard).
    /// </summary>
    public bool Delete(int productOptionId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        try
        {
            using (MySqlCommand cmd = new(
                "DELETE FROM product_allowed_option WHERE product_option_id = @product_option_id;",
                cn, tx))
            {
                cmd.Parameters.AddWithValue("@product_option_id", productOptionId);
                cmd.ExecuteNonQuery();
            }

            using (MySqlCommand cmd = new(
                "DELETE FROM product_option WHERE product_option_id = @product_option_id;",
                cn, tx))
            {
                cmd.Parameters.AddWithValue("@product_option_id", productOptionId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }
        catch (MySqlException ex) when (MySqlErrors.IsRowReferenced(ex))
        {
            tx.Rollback();
            return false;
        }
    }

    private static ProductOption Map(MySqlDataReader reader)
    {
        return new ProductOption
        {
            ProductOptionId = reader.GetInt32(0),
            OptionGroup = reader.GetString(1),
            Name = reader.GetString(2),
            ExtraPrice = reader.GetDecimal(3)
        };
    }
}
