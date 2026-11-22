using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class ProductCategoryRepository
{
    // Column order matters: Map reads by position.
    private const string SelectColumns = "product_category_id, name";

    public List<ProductCategory> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM product_category ORDER BY name;", cn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        List<ProductCategory> categories = new();
        while (reader.Read())
        {
            categories.Add(Map(reader));
        }

        return categories;
    }

    public ProductCategory? GetById(int productCategoryId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM product_category " +
            "WHERE product_category_id = @product_category_id;", cn);
        cmd.Parameters.AddWithValue("@product_category_id", productCategoryId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    /// <summary>
    /// Finds a category by name using the column collation, so the match is
    /// the same one the UNIQUE constraint applies (case and accent insensitive).
    /// </summary>
    public ProductCategory? GetByName(string name)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM product_category WHERE name = @name;", cn);
        cmd.Parameters.AddWithValue("@name", name);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(ProductCategory category)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO product_category (name) VALUES (@name);", cn);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    public void Update(ProductCategory category)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE product_category SET name = @name " +
            "WHERE product_category_id = @product_category_id;", cn);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@product_category_id", category.ProductCategoryId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the category. Returns false, without changes, when products
    /// still reference it (the foreign key is the guard).
    /// </summary>
    public bool Delete(int productCategoryId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "DELETE FROM product_category WHERE product_category_id = @product_category_id;", cn);
        cmd.Parameters.AddWithValue("@product_category_id", productCategoryId);

        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException ex) when (MySqlErrors.IsRowReferenced(ex))
        {
            return false;
        }
    }

    private static ProductCategory Map(MySqlDataReader reader)
    {
        return new ProductCategory
        {
            ProductCategoryId = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    }
}
