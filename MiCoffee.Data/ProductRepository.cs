using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class ProductRepository
{
    // Column order matters: Map reads by position.
    private const string SelectProduct =
        "SELECT p.product_id, p.product_category_id, p.name, p.base_price, " +
        "p.is_available, c.name " +
        "FROM product p " +
        "JOIN product_category c ON c.product_category_id = p.product_category_id ";

    // Column order matters: AttachAllowedOptions reads by position.
    private const string SelectAllowedOptions =
        "SELECT pao.product_id, o.product_option_id, o.option_group, o.name, o.extra_price " +
        "FROM product_allowed_option pao " +
        "JOIN product_option o ON o.product_option_id = pao.product_option_id ";

    /// <summary>
    /// All products ordered by category name and product name. Category is
    /// loaded; AllowedOptions is not.
    /// </summary>
    public List<Product> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(SelectProduct + "ORDER BY c.name, p.name;", cn);
        return ReadProducts(cmd);
    }

    /// <summary>
    /// One product with Category and AllowedOptions loaded, or null.
    /// </summary>
    public Product? GetById(int productId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        Product? product;
        using (MySqlCommand cmd = new(SelectProduct + "WHERE p.product_id = @product_id;", cn))
        {
            cmd.Parameters.AddWithValue("@product_id", productId);
            product = ReadProducts(cmd).FirstOrDefault();
        }

        if (product is null)
        {
            return null;
        }

        using MySqlCommand optionsCmd = new(
            SelectAllowedOptions +
            "WHERE pao.product_id = @product_id ORDER BY o.option_group, o.name;", cn);
        optionsCmd.Parameters.AddWithValue("@product_id", productId);
        AttachAllowedOptions(optionsCmd, new List<Product> { product });

        return product;
    }

    /// <summary>
    /// Available products ordered by category name and product name, each with
    /// Category and AllowedOptions loaded.
    /// </summary>
    public List<Product> GetMenu()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        List<Product> products;
        using (MySqlCommand cmd = new(
            SelectProduct + "WHERE p.is_available = TRUE ORDER BY c.name, p.name;", cn))
        {
            products = ReadProducts(cmd);
        }

        using MySqlCommand optionsCmd = new(
            SelectAllowedOptions +
            "JOIN product p ON p.product_id = pao.product_id " +
            "WHERE p.is_available = TRUE ORDER BY o.option_group, o.name;", cn);
        AttachAllowedOptions(optionsCmd, products);

        return products;
    }

    public int Insert(Product product)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO product (product_category_id, name, base_price, is_available) " +
            "VALUES (@product_category_id, @name, @base_price, @is_available);", cn);
        cmd.Parameters.AddWithValue("@product_category_id", product.ProductCategoryId);
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@base_price", product.BasePrice);
        cmd.Parameters.AddWithValue("@is_available", product.IsAvailable);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    /// <summary>
    /// Updates category, name, base price and availability. Allowed options
    /// are changed only through ReplaceAllowedOptions.
    /// </summary>
    public void Update(Product product)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE product SET product_category_id = @product_category_id, name = @name, " +
            "base_price = @base_price, is_available = @is_available " +
            "WHERE product_id = @product_id;", cn);
        cmd.Parameters.AddWithValue("@product_category_id", product.ProductCategoryId);
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@base_price", product.BasePrice);
        cmd.Parameters.AddWithValue("@is_available", product.IsAvailable);
        cmd.Parameters.AddWithValue("@product_id", product.ProductId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the product and its allowed options, in one transaction.
    /// Returns false, without changes, when an order still references the
    /// product (the foreign key is the guard).
    /// </summary>
    public bool Delete(int productId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        try
        {
            using (MySqlCommand cmd = new(
                "DELETE FROM product_allowed_option WHERE product_id = @product_id;", cn, tx))
            {
                cmd.Parameters.AddWithValue("@product_id", productId);
                cmd.ExecuteNonQuery();
            }

            using (MySqlCommand cmd = new(
                "DELETE FROM product WHERE product_id = @product_id;", cn, tx))
            {
                cmd.Parameters.AddWithValue("@product_id", productId);
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

    /// <summary>
    /// Replaces the options allowed for the product, in one transaction.
    /// Repeated ids are ignored. An id that does not exist raises a
    /// MySqlException and leaves the previous assignments untouched; Business
    /// validates the ids before calling.
    /// </summary>
    public void ReplaceAllowedOptions(int productId, IEnumerable<int> productOptionIds)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        using (MySqlCommand delete = new(
            "DELETE FROM product_allowed_option WHERE product_id = @product_id;", cn, tx))
        {
            delete.Parameters.AddWithValue("@product_id", productId);
            delete.ExecuteNonQuery();
        }

        using MySqlCommand insert = new(
            "INSERT INTO product_allowed_option (product_id, product_option_id) " +
            "VALUES (@product_id, @product_option_id);", cn, tx);
        insert.Parameters.AddWithValue("@product_id", productId);
        MySqlParameter optionParameter = insert.Parameters.AddWithValue("@product_option_id", 0);

        foreach (int productOptionId in productOptionIds.Distinct())
        {
            optionParameter.Value = productOptionId;
            insert.ExecuteNonQuery();
        }

        tx.Commit();
    }

    // The reader is closed when this method returns, so the caller can run
    // another command on the same connection.
    private static List<Product> ReadProducts(MySqlCommand cmd)
    {
        using MySqlDataReader reader = cmd.ExecuteReader();
        List<Product> products = new();
        while (reader.Read())
        {
            products.Add(Map(reader));
        }

        return products;
    }

    private static void AttachAllowedOptions(MySqlCommand cmd, List<Product> products)
    {
        Dictionary<int, Product> byId = products.ToDictionary(p => p.ProductId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            // A product created between the two queries is not in the list.
            if (!byId.TryGetValue(reader.GetInt32(0), out Product? product))
            {
                continue;
            }

            product.AllowedOptions.Add(new ProductOption
            {
                ProductOptionId = reader.GetInt32(1),
                OptionGroup = reader.GetString(2),
                Name = reader.GetString(3),
                ExtraPrice = reader.GetDecimal(4)
            });
        }
    }

    private static Product Map(MySqlDataReader reader)
    {
        return new Product
        {
            ProductId = reader.GetInt32(0),
            ProductCategoryId = reader.GetInt32(1),
            Name = reader.GetString(2),
            BasePrice = reader.GetDecimal(3),
            IsAvailable = reader.GetBoolean(4),
            Category = new ProductCategory
            {
                ProductCategoryId = reader.GetInt32(1),
                Name = reader.GetString(5)
            }
        };
    }
}
