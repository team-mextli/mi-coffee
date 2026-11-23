using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class SupplyCategoryRepository
{
    // Column order matters: Map reads by position.
    private const string SelectColumns = "supply_category_id, name";

    public List<SupplyCategory> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM supply_category ORDER BY name;", cn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        List<SupplyCategory> categories = new();
        while (reader.Read())
        {
            categories.Add(Map(reader));
        }

        return categories;
    }

    public SupplyCategory? GetById(int supplyCategoryId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM supply_category " +
            "WHERE supply_category_id = @supply_category_id;", cn);
        cmd.Parameters.AddWithValue("@supply_category_id", supplyCategoryId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    /// <summary>
    /// Finds a category by name using the column collation, so the match is
    /// the same one the UNIQUE constraint applies (case and accent insensitive).
    /// </summary>
    public SupplyCategory? GetByName(string name)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM supply_category WHERE name = @name;", cn);
        cmd.Parameters.AddWithValue("@name", name);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(SupplyCategory category)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO supply_category (name) VALUES (@name);", cn);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    public void Update(SupplyCategory category)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE supply_category SET name = @name " +
            "WHERE supply_category_id = @supply_category_id;", cn);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@supply_category_id", category.SupplyCategoryId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the category. Returns false, without changes, when supplies
    /// still reference it (the foreign key is the guard).
    /// </summary>
    public bool Delete(int supplyCategoryId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "DELETE FROM supply_category WHERE supply_category_id = @supply_category_id;", cn);
        cmd.Parameters.AddWithValue("@supply_category_id", supplyCategoryId);

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

    private static SupplyCategory Map(MySqlDataReader reader)
    {
        return new SupplyCategory
        {
            SupplyCategoryId = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    }
}