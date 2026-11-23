using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class SupplyRepository
{
    // Column order matters: Map reads by position. The supplier is optional,
    // hence the LEFT JOIN: a supply without supplier still appears.
    private const string SelectSupply =
        "SELECT s.supply_id, s.supply_category_id, s.supplier_id, s.name, s.unit, " +
        "s.current_stock, s.minimum_stock, c.name, sp.name, sp.contact, sp.note " +
        "FROM supply s " +
        "JOIN supply_category c ON c.supply_category_id = s.supply_category_id " +
        "LEFT JOIN supplier sp ON sp.supplier_id = s.supplier_id ";

    /// <summary>
    /// All supplies ordered by category name and supply name, with Category
    /// and Supplier loaded.
    /// </summary>
    public List<Supply> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(SelectSupply + "ORDER BY c.name, s.name;", cn);
        return ReadSupplies(cmd);
    }

    public Supply? GetById(int supplyId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(SelectSupply + "WHERE s.supply_id = @supply_id;", cn);
        cmd.Parameters.AddWithValue("@supply_id", supplyId);
        return ReadSupplies(cmd).FirstOrDefault();
    }

    /// <summary>
    /// Supplies whose stock is at or below the minimum, largest shortage
    /// first and then by name. The alert is computed here; it is not stored.
    /// </summary>
    public List<Supply> GetAlerts()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            SelectSupply +
            "WHERE s.current_stock <= s.minimum_stock " +
            "ORDER BY (s.minimum_stock - s.current_stock) DESC, s.name;", cn);
        return ReadSupplies(cmd);
    }

    public int Insert(Supply supply)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO supply (supply_category_id, supplier_id, name, unit, " +
            "current_stock, minimum_stock) " +
            "VALUES (@supply_category_id, @supplier_id, @name, @unit, " +
            "@current_stock, @minimum_stock);", cn);
        cmd.Parameters.AddWithValue("@supply_category_id", supply.SupplyCategoryId);
        cmd.Parameters.AddWithValue("@supplier_id", (object?)supply.SupplierId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@name", supply.Name);
        cmd.Parameters.AddWithValue("@unit", supply.Unit);
        cmd.Parameters.AddWithValue("@current_stock", supply.CurrentStock);
        cmd.Parameters.AddWithValue("@minimum_stock", supply.MinimumStock);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    /// <summary>
    /// Updates everything except current_stock, which changes only through
    /// AddStock and SetStock.
    /// </summary>
    public void Update(Supply supply)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE supply SET supply_category_id = @supply_category_id, " +
            "supplier_id = @supplier_id, name = @name, unit = @unit, " +
            "minimum_stock = @minimum_stock " +
            "WHERE supply_id = @supply_id;", cn);
        cmd.Parameters.AddWithValue("@supply_category_id", supply.SupplyCategoryId);
        cmd.Parameters.AddWithValue("@supplier_id", (object?)supply.SupplierId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@name", supply.Name);
        cmd.Parameters.AddWithValue("@unit", supply.Unit);
        cmd.Parameters.AddWithValue("@minimum_stock", supply.MinimumStock);
        cmd.Parameters.AddWithValue("@supply_id", supply.SupplyId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Adds quantity to the current stock in a single statement, so two
    /// simultaneous entries do not overwrite each other.
    /// </summary>
    public void AddStock(int supplyId, decimal quantity)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE supply SET current_stock = current_stock + @quantity " +
            "WHERE supply_id = @supply_id;", cn);
        cmd.Parameters.AddWithValue("@quantity", quantity);
        cmd.Parameters.AddWithValue("@supply_id", supplyId);
        cmd.ExecuteNonQuery();
    }

    public void SetStock(int supplyId, decimal newStock)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE supply SET current_stock = @current_stock " +
            "WHERE supply_id = @supply_id;", cn);
        cmd.Parameters.AddWithValue("@current_stock", newStock);
        cmd.Parameters.AddWithValue("@supply_id", supplyId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the supply. No other table references it, so nothing blocks it.
    /// </summary>
    public void Delete(int supplyId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "DELETE FROM supply WHERE supply_id = @supply_id;", cn);
        cmd.Parameters.AddWithValue("@supply_id", supplyId);
        cmd.ExecuteNonQuery();
    }

    private static List<Supply> ReadSupplies(MySqlCommand cmd)
    {
        using MySqlDataReader reader = cmd.ExecuteReader();
        List<Supply> supplies = new();
        while (reader.Read())
        {
            supplies.Add(Map(reader));
        }

        return supplies;
    }

    private static Supply Map(MySqlDataReader reader)
    {
        int? supplierId = reader.IsDBNull(2) ? null : reader.GetInt32(2);

        return new Supply
        {
            SupplyId = reader.GetInt32(0),
            SupplyCategoryId = reader.GetInt32(1),
            SupplierId = supplierId,
            Name = reader.GetString(3),
            Unit = reader.GetString(4),
            CurrentStock = reader.GetDecimal(5),
            MinimumStock = reader.GetDecimal(6),
            Category = new SupplyCategory
            {
                SupplyCategoryId = reader.GetInt32(1),
                Name = reader.GetString(7)
            },
            Supplier = supplierId is null
                ? null
                : new Supplier
                {
                    SupplierId = supplierId.Value,
                    Name = reader.GetString(8),
                    Contact = reader.GetString(9),
                    Note = reader.IsDBNull(10) ? null : reader.GetString(10)
                }
        };
    }
}